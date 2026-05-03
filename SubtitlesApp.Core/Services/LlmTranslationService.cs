using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Services;

public class LlmTranslationService(ILlmTranslationSettings settings, ILlmClient llmClient) : ITranslationService
{
    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async IAsyncEnumerable<Result<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> sourceSubtitles,
        Language targetLanguage,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        foreach (var chunk in sourceSubtitles.Chunk(settings.ChunkSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var translatedSubs = await TranslateAsyncInternal([.. chunk], targetLanguage, cancellationToken);

            if (translatedSubs.IsFailure)
            {
                yield return Result<SubtitleDto>.Failure(translatedSubs.Error);
                yield break;
            }

            foreach (var subtitle in translatedSubs.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return Result<SubtitleDto>.Success(subtitle);
            }
        }
    }

    #region Private Methods

    private async Task<ListResult<SubtitleDto>> TranslateAsyncInternal(
        List<SubtitleDto> sourceSubtitles,
        Language targetLanguage,
        CancellationToken cancellationToken
    )
    {
        List<LlmMessageDto> chatHistory = [new(LlmRoleConstants.System, settings.DefaultSystemPrompt)];

        var retryCounter = 0;

        while (retryCounter <= settings.RetryCount)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ListResult<SubtitleDto>.Failure(new Error(ErrorCode.OperationCanceled));
            }

            var userPrompt = FormUserPrompt(targetLanguage.Name, sourceSubtitles);
            Result<LlmSubtitleListDto> llmResult;

            try
            {
                llmResult = await llmClient.SendChatAsync<LlmSubtitleListDto>(chatHistory, userPrompt);
            }
            catch (Exception ex)
            {
                llmResult = Result<LlmSubtitleListDto>.Failure(
                    new Error(ErrorCode.InternalClientError, $"LLM translation failed with error: {ex.Message}")
                );
            }

            if (llmResult.IsFailure && retryCounter <= settings.RetryCount)
            {
                retryCounter++;
                continue;
            }
            else if (llmResult.IsFailure)
            {
                return ListResult<SubtitleDto>.Failure(llmResult.Error);
            }

            var llmSubtitles = llmResult.Value.Items;
            var isTranlationValid = llmSubtitles.Count == sourceSubtitles.Count && IsTranlsationValid(llmSubtitles);

            if (!isTranlationValid && retryCounter <= settings.RetryCount)
            {
                retryCounter++;
                continue;
            }
            else if (!isTranlationValid)
            {
                return ListResult<SubtitleDto>.Failure(new Error(ErrorCode.InvalidLlmTranslation));
            }

            var translatedSubs = MapTranslationsToSubs(targetLanguage.Code, llmSubtitles, sourceSubtitles);

            return ListResult<SubtitleDto>.Success(translatedSubs);
        }

        return ListResult<SubtitleDto>.Failure(new Error(ErrorCode.RetryLimitExceeded));
    }

    private static string FormUserPrompt(string targetLang, List<SubtitleDto> sourceSubs)
    {
        int id = 1;
        var llmSubsList = new LlmSubtitleListDto { Items = [] };

        foreach (var srcSub in sourceSubs)
        {
            llmSubsList.Items.Add(new() { Id = id, Text = srcSub.Text });
            id++;
        }

        var serializedSubs = JsonSerializer.Serialize(llmSubsList, _writeOptions);

        return string.Format("Translate to {0}.\n\n{1}.", targetLang, serializedSubs);
    }

    private static List<SubtitleDto> MapTranslationsToSubs(
        string targetLangCode,
        List<LlmSubtitleDto> llmSubtitles,
        List<SubtitleDto> sourceSubs
    )
    {
        List<SubtitleDto> results = [];

        foreach (var (srcSub, llmSub) in sourceSubs.Zip(llmSubtitles))
        {
            results.Add(
                new()
                {
                    LanguageCode = targetLangCode,
                    Text = llmSub.Text,
                    StartTime = srcSub.StartTime,
                    EndTime = srcSub.EndTime,
                }
            );
        }

        return results;
    }

    private static bool IsTranlsationValid(List<LlmSubtitleDto> llmSubtitles)
    {
        for (int i = 0; i < llmSubtitles.Count; i++)
        {
            if (llmSubtitles[i].Id != i + 1)
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
