using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.ExternalClients;
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

    public async IAsyncEnumerable<Result<Subtitle>> TranslateAsync(
        List<Subtitle> sourceSubtitles,
        Language targetLanguage,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var context = new List<Subtitle>();

        foreach (var chunk in sourceSubtitles.Chunk(settings.ChunkSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var translatedSubs = await TranslateAsyncInternal(chunk, context, targetLanguage, cancellationToken);

            if (translatedSubs.IsFailure)
            {
                yield return Result<Subtitle>.Failure(translatedSubs.Error);
                yield break;
            }

            UpdateContext(context, chunk);

            foreach (var subtitle in translatedSubs.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return Result<Subtitle>.Success(subtitle);
            }
        }
    }

    #region Private Methods

    private async Task<ListResult<Subtitle>> TranslateAsyncInternal(
        Subtitle[] sourceSubtitles,
        List<Subtitle> context,
        Language targetLanguage,
        CancellationToken cancellationToken
    )
    {
        List<LlmMessageDto> chatHistory = [new(LlmRoleConstants.System, settings.DefaultSystemPrompt)];

        if (cancellationToken.IsCancellationRequested)
        {
            return ListResult<Subtitle>.Failure(new Error(ErrorCode.OperationCanceled));
        }

        var userPrompt = FormUserPrompt(targetLanguage.Name, sourceSubtitles, context);
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

        if (llmResult.IsFailure)
        {
            return ListResult<Subtitle>.Failure(llmResult.Error);
        }

        var llmSubtitles = llmResult.Value.Items;
        var isTranlationValid = llmSubtitles.Count == sourceSubtitles.Length && IsTranlsationValid(llmSubtitles);

        if (!isTranlationValid)
        {
            return ListResult<Subtitle>.Failure(new Error(ErrorCode.InvalidLlmTranslation));
        }

        var translatedSubs = MapTranslationsToSubs(targetLanguage.Code, llmSubtitles, sourceSubtitles);

        return ListResult<Subtitle>.Success(translatedSubs);
    }

    private static string FormUserPrompt(string targetLang, Subtitle[] sourceSubs, List<Subtitle> context)
    {
        int id = 1;
        var llmSubsList = new LlmSubtitleListDto { Items = [] };

        foreach (var srcSub in sourceSubs)
        {
            llmSubsList.Items.Add(new() { Id = id, Text = srcSub.Text });
            id++;
        }

        var serializedSubs = JsonSerializer.Serialize(llmSubsList, _writeOptions);

        if (context.Count > 0)
        {
            var mappedContext = context.Select(x => new { x.Text });
            var serializedContext = JsonSerializer.Serialize(mappedContext, _writeOptions);

            return string.Format(
                "Translate to {0}.\n\nContext:\n{1}\n\nSource items:\n{2}",
                targetLang,
                serializedContext,
                serializedSubs
            );
        }
        else
        {
            return string.Format("Translate to {0}.\n\nSource items:\n{1}", targetLang, serializedSubs);
        }
    }

    private static List<Subtitle> MapTranslationsToSubs(
        string targetLangCode,
        List<LlmSubtitleDto> llmSubtitles,
        Subtitle[] sourceSubs
    )
    {
        List<Subtitle> results = [];

        foreach (var (srcSub, llmSub) in sourceSubs.Zip(llmSubtitles))
        {
            results.Add(
                new()
                {
                    LanguageCode = targetLangCode,
                    Text = llmSub.Text,
                    TimeInterval = srcSub.TimeInterval,
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

    private static void UpdateContext(List<Subtitle> context, Subtitle[] sourceSubtitles)
    {
        context.Clear();

        if (sourceSubtitles.Length == 0)
        {
            return;
        }
        else if (sourceSubtitles.Length == 1)
        {
            context.Add(sourceSubtitles[0]);
        }
        else
        {
            context.Add(sourceSubtitles[^2]);
            context.Add(sourceSubtitles[^1]);
        }
    }

    #endregion
}
