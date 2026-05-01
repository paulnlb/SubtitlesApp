using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Services;

public class LlmTranslationService(ILlmTranslationSettings settings, LanguageService languageService, ILlmClient llmClient)
    : ITranslationService
{
    private readonly List<LlmMessageDto> _chatHistory = [new("system", settings.DefaultSystemPrompt)];

    public async IAsyncEnumerable<Result<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> sourceSubtitles,
        string targetLanguageCode,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var targetLanguage = languageService.GetLanguageByCode(targetLanguageCode);

        if (targetLanguage == null)
        {
            var error = new Error(
                ErrorCode.BadRequest,
                $"Target language code {targetLanguageCode} is invalid or not supported"
            );

            yield return Result<SubtitleDto>.Failure(error);
            yield break;
        }

        var i = 0;
        var retryCounter = 0;

        while (i < sourceSubtitles.Count)
        {
            var sub = sourceSubtitles[i];

            if (cancellationToken.IsCancellationRequested)
            {
                yield return Result<SubtitleDto>.Failure(new Error(ErrorCode.OperationCanceled));

                break;
            }

            var userMsg = GetUserMsg(targetLanguage.Name, sub.Text);
            Result<TranslationDto> llmResult;

            try
            {
                llmResult = await llmClient.SendChatAsync<TranslationDto>(_chatHistory, userMsg.Content);
            }
            catch (Exception ex)
            {
                llmResult = Result<TranslationDto>.Failure(
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
                yield return Result<SubtitleDto>.Failure(llmResult.Error);

                break;
            }

            var translatedText = llmResult.Value.TranslatedText;

            _chatHistory.Add(userMsg);
            _chatHistory.Add(new("assistant", translatedText));

            yield return Result<SubtitleDto>.Success(
                new SubtitleDto
                {
                    LanguageCode = targetLanguageCode,
                    Text = translatedText,
                    StartTime = sub.StartTime,
                    EndTime = sub.EndTime,
                }
            );

            i++;
        }
    }

    #region Private Methods

    private static LlmMessageDto GetUserMsg(string targetLang, string text)
    {
        return new LlmMessageDto(
            "user",
            $"""
            Target language: {targetLang}.

            Text: {text}.
            """
        );
    }

    #endregion
}
