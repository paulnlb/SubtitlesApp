using System.Text;
using Catalyst;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.WhisperApi.Models;
using SubtitlesServer.WhisperApi.Services.Interfaces;
using Whisper.net;

namespace SubtitlesServer.WhisperApi.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    WhisperModelService whisperModelService,
    LanguageService languageService,
    CatalystModelService catalystModelService
) : ITranscriptionService
{
    public async Task<List<SubtitleDto>> TranscribeAudioAsync(
        WhisperRequestModel whisperRequestModel,
        CancellationToken cancellationToken = default
    )
    {
        var factory = await whisperModelService.GetWhisperFactoryAsync();

        if (string.IsNullOrEmpty(whisperRequestModel.LanguageCode))
        {
            whisperRequestModel.LanguageCode = languageService.GetDefaultLanguage().Code;
        }

        var processor = BuildWhisperProcessor(factory, whisperRequestModel);

        using var audioStream = new MemoryStream();
        await whisperRequestModel.AudioFile.CopyToAsync(audioStream, cancellationToken);
        audioStream.Seek(0, SeekOrigin.Begin);

        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        logger.LogInformation(
            "Starting transcribing... Audio textLanguage: {textLanguage}",
            whisperRequestModel.LanguageCode
        );

        var subtitlesList = new List<SubtitleDto>();

        try
        {
            await foreach (var result in MapToSubtitles(segments))
            {
                subtitlesList.Add(result);
            }

            if (whisperRequestModel.OneSentencePerSubtitle)
            {
                subtitlesList = await FormSentenceSubtitles(subtitlesList);
            }

            return subtitlesList;
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }

    private async Task<List<SubtitleDto>> FormSentenceSubtitles(List<SubtitleDto> subtitlesList)
    {
        var resultSubtitles = new List<SubtitleDto>();

        var (mergedText, textLanguage) = MergeAllText(subtitlesList);
        var sentences = SplitToSentences(mergedText, textLanguage);

        var sentencesEnumerator = sentences.GetAsyncEnumerator();
        if (!await sentencesEnumerator.MoveNextAsync())
        {
            return resultSubtitles;
        }

        var cumulativeTextLength = 0;
        SubtitleDto? resultSubtitle = null;
        var currentSentence = sentencesEnumerator.Current;

        foreach (var sourceSubtitle in subtitlesList)
        {
            if (resultSubtitle == null)
            {
                resultSubtitle = sourceSubtitle;

                // Remove leading whitespace for the first subtitle
                resultSubtitle.Text = resultSubtitle.Text.TrimStart();
            }

            cumulativeTextLength += sourceSubtitle.Text.Length;
            resultSubtitle.EndTime = sourceSubtitle.EndTime;

            if (cumulativeTextLength == currentSentence.Length)
            {
                resultSubtitle.Text = currentSentence;
                resultSubtitles.Add(resultSubtitle);

                await sentencesEnumerator.MoveNextAsync();

                cumulativeTextLength = 0;
                resultSubtitle = null;
                currentSentence = sentencesEnumerator.Current;
            }
            else if (cumulativeTextLength > currentSentence.Length)
            {
                throw new InvalidOperationException(
                    $"Cannot merge per word subtitles to form this sentence: \"{currentSentence}\".\n\n"
                        + $"Sentence length is {currentSentence.Length}, while cumulative length of per word subtitles is {cumulativeTextLength}"
                );
            }
        }

        return resultSubtitles;
    }

    private async IAsyncEnumerable<string> SplitToSentences(string text, string languageCode)
    {
        var pipeline = await catalystModelService.GetPipelineAsync(languageCode, fallbackLanguage: LanguageCodes.English);
        var doc = new Document(text, pipeline.Language);
        pipeline.ProcessSingle(doc);

        foreach (var sentence in doc.Spans.Select(sp => sp.Value))
        {
            yield return sentence;
        }
    }

    private static WhisperProcessor BuildWhisperProcessor(WhisperFactory factory, WhisperRequestModel whisperRequestModel)
    {
        var whisperBuilder = factory.CreateBuilder().WithLanguage(whisperRequestModel.LanguageCode);

        if (whisperRequestModel.OneSentencePerSubtitle)
        {
            whisperRequestModel.MaxSegmentLength = 1;
        }

        if (whisperRequestModel.MaxSegmentLength > 0)
        {
            whisperBuilder.WithTokenTimestamps().SplitOnWord().WithMaxSegmentLength(whisperRequestModel.MaxSegmentLength);
        }

        return whisperBuilder.Build();
    }

    /// <summary>
    ///     Maps each segment to <see cref="SubtitleDto"/> and removes subtitles with duplicated text from the resulting collection
    /// </summary>
    /// <param name="segments"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<SubtitleDto> MapToSubtitles(IAsyncEnumerable<SegmentData> segments)
    {
        SubtitleDto? lastSubtitle = null;

        await foreach (var segment in segments)
        {
            var subtitle = new SubtitleDto()
            {
                Text = segment.Text,
                StartTime = segment.Start,
                EndTime = segment.End,
                LanguageCode = segment.Language,
            };

            if (lastSubtitle == null)
            {
                lastSubtitle = subtitle;
                continue;
            }

            if (lastSubtitle.Text != subtitle.Text)
            {
                yield return lastSubtitle;
                lastSubtitle = subtitle;
            }
            else
            {
                lastSubtitle.EndTime = subtitle.EndTime;
            }
        }

        if (lastSubtitle != null)
        {
            yield return lastSubtitle;
        }
    }

    private static (string MergedText, string TextLanguage) MergeAllText(List<SubtitleDto> subtitlesList)
    {
        var languagesCounts = new Dictionary<string, int>();
        var text = new StringBuilder();
        foreach (var currentSub in subtitlesList)
        {
            if (!languagesCounts.TryGetValue(currentSub.LanguageCode, out _))
            {
                languagesCounts.Add(currentSub.LanguageCode, 0);
            }

            languagesCounts[currentSub.LanguageCode] += 1;

            text.Append(currentSub.Text);
        }

        var textLanguage = languagesCounts.OrderBy(e => e.Value).Select(e => e.Key).FirstOrDefault();

        textLanguage ??= string.Empty;

        return (text.ToString(), textLanguage);
    }
}
