using OpenAiMockServer.ResponseModels;

namespace OpenAiMockServer.Helpers;

public static class SeedHelper
{
    public static IEnumerable<Segment> MakeSegments(string[] words, int maxSegments, int maxWordsPerSegment)
    {
        var random = new Random();
        int cursor = 0;
        int step = random.Next(1, maxWordsPerSegment);

        int startTime = 0;
        int endTime = 2;

        if (step > words.Length)
        {
            step = words.Length;
        }

        int emittedCount = 0;

        while (cursor + step <= words.Length && emittedCount < maxSegments)
        {
            var currentText = $"{emittedCount}: " + string.Join(" ", words.Skip(cursor).Take(step));

            yield return new Segment
            {
                Start = startTime,
                End = endTime,
                Text = currentText,
            };

            emittedCount++;

            cursor += step;
            step = random.Next(1, maxWordsPerSegment);
            startTime = endTime;
            endTime += 2;
        }
    }

    public static IEnumerable<LlmSubtitleDto> MakeTranslations(string[] words, int maxSegments, int maxWordsPerSegment)
    {
        var random = new Random();
        int cursor = 0;
        int step = random.Next(1, maxWordsPerSegment);

        if (step > words.Length)
        {
            step = words.Length;
        }

        int emittedCount = 0;

        while (cursor + step <= words.Length && emittedCount < maxSegments)
        {
            var currentText = $"{emittedCount}: " + string.Join(" ", words.Skip(cursor).Take(step));

            yield return new LlmSubtitleDto { Id = emittedCount + 1, Text = currentText };

            emittedCount++;

            cursor += step;
            step = random.Next(1, maxWordsPerSegment);
        }
    }
}
