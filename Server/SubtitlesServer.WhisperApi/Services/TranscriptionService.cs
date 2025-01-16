using System.Text;
using AutoMapper;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Services;

public class TranscriptionService(ISpeechToTextService speechToTextService, INlpService nlpService, IMapper mapper)
    : ITranscriptionService
{
    public async Task<List<SubtitleDto>> TranscribeAudioAsync(
        WhisperRequestModel whisperRequestModel,
        CancellationToken cancellationToken = default
    )
    {
        if (whisperRequestModel.OneSentencePerSubtitle)
        {
            whisperRequestModel.MaxSegmentLength = 1;
        }

        var whisperDto = await MapToRequestToDto(whisperRequestModel, cancellationToken);
        var subtitlesList = new List<SubtitleDto>();

        var subtitlesEnumerable = speechToTextService.TranscribeAudioAsync(whisperDto, cancellationToken);

        await foreach (var subtitle in RemoveDuplicates(subtitlesEnumerable))
        {
            subtitlesList.Add(subtitle);
        }

        if (whisperRequestModel.OneSentencePerSubtitle)
        {
            subtitlesList = await FormSentenceSubtitles(subtitlesList);
        }

        return subtitlesList;
    }

    private async Task<List<SubtitleDto>> FormSentenceSubtitles(List<SubtitleDto> subtitlesList)
    {
        var resultSubtitles = new List<SubtitleDto>();

        var (mergedText, textLanguage) = MergeAllText(subtitlesList);
        var sentences = nlpService.SplitToSentences(mergedText, textLanguage);

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

    private async Task<WhisperDto> MapToRequestToDto(
        WhisperRequestModel whisperRequestModel,
        CancellationToken cancellationToken
    )
    {
        var whisperDto = mapper.Map<WhisperDto>(whisperRequestModel);
        whisperDto.AudioStream = new MemoryStream();
        await whisperRequestModel.AudioFile.CopyToAsync(whisperDto.AudioStream, cancellationToken);
        whisperDto.AudioStream.Seek(0, SeekOrigin.Begin);

        return whisperDto;
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

    private static async IAsyncEnumerable<SubtitleDto> RemoveDuplicates(IAsyncEnumerable<SubtitleDto> subtitltes)
    {
        SubtitleDto? lastSubtitle = null;

        await foreach (var subtitle in subtitltes)
        {
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
}
