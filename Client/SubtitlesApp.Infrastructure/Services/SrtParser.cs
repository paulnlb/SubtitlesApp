using System.Globalization;
using System.Runtime.CompilerServices;

namespace SubtitlesApp.Infrastructure.Services;

/// <summary>
/// Represents a single SubRip (.srt) subtitle block.
/// </summary>
public sealed record SrtItem
{
    public int Index { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string Text { get; init; } = string.Empty;

    public SrtItem() { }

    public SrtItem(int index, TimeSpan startTime, TimeSpan endTime, string text)
    {
        Index = index;
        StartTime = startTime;
        EndTime = endTime;
        Text = text ?? string.Empty;
    }
}

public sealed class SrtParserOptions
{
    /// <summary>
    /// When true, ignores malformed timestamp entries or missing indexes instead of throwing a FormatException.
    /// </summary>
    public bool Tolerant { get; set; } = true;

    /// <summary>
    /// Line separator used when joining multi-line subtitle text. Defaults to "\n".
    /// </summary>
    public string TargetNewLine { get; set; } = "\n";
}

public sealed class SrtSerializerOptions
{
    /// <summary>
    /// Newline string for SRT output. Standard SRT spec mandates CRLF ("\r\n").
    /// </summary>
    public string NewLine { get; set; } = "\r\n";

    /// <summary>
    /// Automatically renumbers entries sequentially starting from 1 during serialization.
    /// </summary>
    public bool Renumber { get; set; } = true;

    /// <summary>
    /// Optional uniform time shift applied to all timestamps during serialization.
    /// </summary>
    public TimeSpan TimeOffset { get; set; } = TimeSpan.Zero;
}

public static class SrtParser
{
    public static IEnumerable<SrtItem> Parse(string srtContent, SrtParserOptions? options = null)
    {
        if (string.IsNullOrEmpty(srtContent))
            yield break;

        using var reader = new StringReader(srtContent);
        foreach (var item in Parse(reader, options))
        {
            yield return item;
        }
    }

    public static IEnumerable<SrtItem> Parse(TextReader reader, SrtParserOptions? options = null)
    {
        options ??= new SrtParserOptions();
        int autoIndex = 1;

        string? currentLine = reader.ReadLine();

        // Strip UTF-8 BOM if present
        if (currentLine != null && currentLine.Length > 0 && currentLine[0] == '\uFEFF')
        {
            currentLine = currentLine[1..];
        }

        while (currentLine != null)
        {
            if (string.IsNullOrWhiteSpace(currentLine))
            {
                currentLine = reader.ReadLine();
                continue;
            }

            int index = autoIndex;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            bool hasTimeCode = false;

            ReadOnlySpan<char> lineSpan = currentLine.AsSpan().Trim();

            // Check if current line is index integer
            if (int.TryParse(lineSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedIndex))
            {
                index = parsedIndex;
                currentLine = reader.ReadLine();
                if (currentLine == null)
                    break;
                lineSpan = currentLine.AsSpan().Trim();
            }

            // Parse timestamp line: HH:mm:ss,fff --> HH:mm:ss,fff
            if (TryParseTimeCodeLine(lineSpan, out startTime, out endTime))
            {
                hasTimeCode = true;
            }
            else if (options.Tolerant)
            {
                currentLine = reader.ReadLine();
                continue;
            }
            else
            {
                throw new FormatException($"Invalid SRT timestamp line: '{currentLine}'");
            }

            // Read text lines until blank line or next block timestamp
            var textLines = new List<string>(2);
            currentLine = reader.ReadLine();

            while (currentLine != null)
            {
                ReadOnlySpan<char> textSpan = currentLine.AsSpan().Trim();

                if (textSpan.IsEmpty)
                {
                    currentLine = reader.ReadLine();
                    break;
                }

                // Lookahead check for missing blank lines between subtitle blocks
                if (textSpan.Contains("-->".AsSpan(), StringComparison.Ordinal))
                {
                    break;
                }

                textLines.Add(currentLine);
                currentLine = reader.ReadLine();
            }

            if (hasTimeCode)
            {
                string text = string.Join(options.TargetNewLine, textLines);
                yield return new SrtItem(index, startTime, endTime, text);
                autoIndex = index + 1;
            }
        }
    }

    public static async IAsyncEnumerable<SrtItem> ParseAsync(
        TextReader reader,
        SrtParserOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        options ??= new SrtParserOptions();
        int autoIndex = 1;

        string? currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

        if (currentLine != null && currentLine.Length > 0 && currentLine[0] == '\uFEFF')
        {
            currentLine = currentLine[1..];
        }

        while (currentLine != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(currentLine))
            {
                currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                continue;
            }

            int index = autoIndex;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            bool hasTimeCode = false;

            ReadOnlySpan<char> lineSpan = currentLine.AsSpan().Trim();

            if (int.TryParse(lineSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedIndex))
            {
                index = parsedIndex;
                currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (currentLine == null)
                    break;
                lineSpan = currentLine.AsSpan().Trim();
            }

            if (TryParseTimeCodeLine(lineSpan, out startTime, out endTime))
            {
                hasTimeCode = true;
            }
            else if (options.Tolerant)
            {
                currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                continue;
            }
            else
            {
                throw new FormatException($"Invalid SRT timestamp line: '{currentLine}'");
            }

            var textLines = new List<string>(2);
            currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            while (currentLine != null)
            {
                ReadOnlySpan<char> textSpan = currentLine.AsSpan().Trim();

                if (textSpan.IsEmpty)
                {
                    currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    break;
                }

                if (textSpan.Contains("-->".AsSpan(), StringComparison.Ordinal))
                {
                    break;
                }

                textLines.Add(currentLine);
                currentLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }

            if (hasTimeCode)
            {
                string text = string.Join(options.TargetNewLine, textLines);
                yield return new SrtItem(index, startTime, endTime, text);
                autoIndex = index + 1;
            }
        }
    }

    private static bool TryParseTimeCodeLine(ReadOnlySpan<char> span, out TimeSpan startTime, out TimeSpan endTime)
    {
        startTime = TimeSpan.Zero;
        endTime = TimeSpan.Zero;

        int arrowIdx = span.IndexOf("-->".AsSpan(), StringComparison.Ordinal);
        if (arrowIdx < 0)
            return false;

        ReadOnlySpan<char> startSpan = span[..arrowIdx].Trim();
        ReadOnlySpan<char> endSpan = span[(arrowIdx + 3)..].Trim();

        // Strip trailing SSA/vobsub alignment codes if present (e.g. X1:000 Y1:000)
        int spaceIdx = endSpan.IndexOf(' ');
        if (spaceIdx > 0)
        {
            endSpan = endSpan[..spaceIdx];
        }

        return TryParseTimestamp(startSpan, out startTime) && TryParseTimestamp(endSpan, out endTime);
    }

    private static bool TryParseTimestamp(ReadOnlySpan<char> span, out TimeSpan timeSpan)
    {
        timeSpan = TimeSpan.Zero;
        span = span.Trim();

        int col1 = span.IndexOf(':');
        if (col1 <= 0)
            return false;

        int col2 = span[(col1 + 1)..].IndexOf(':');
        if (col2 <= 0)
            return false;
        col2 += col1 + 1;

        int sep = span[(col2 + 1)..].IndexOfAny(',', '.');
        int sepPos = (sep >= 0) ? (col2 + 1 + sep) : span.Length;

        if (!int.TryParse(span[..col1], NumberStyles.None, CultureInfo.InvariantCulture, out int hours))
            return false;
        if (!int.TryParse(span[(col1 + 1)..col2], NumberStyles.None, CultureInfo.InvariantCulture, out int minutes))
            return false;

        ReadOnlySpan<char> secSpan = (sepPos < span.Length) ? span[(col2 + 1)..sepPos] : span[(col2 + 1)..];
        if (!int.TryParse(secSpan, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds))
            return false;

        int millis = 0;
        if (sepPos < span.Length)
        {
            ReadOnlySpan<char> msSpan = span[(sepPos + 1)..];
            if (msSpan.Length > 3)
                msSpan = msSpan[..3];
            if (!int.TryParse(msSpan, NumberStyles.None, CultureInfo.InvariantCulture, out millis))
                return false;

            if (msSpan.Length == 1)
                millis *= 100;
            else if (msSpan.Length == 2)
                millis *= 10;
        }

        timeSpan = new TimeSpan(0, hours, minutes, seconds, millis);
        return true;
    }
}

public static class SrtSerializer
{
    public static string Serialize(IEnumerable<SrtItem> items, SrtSerializerOptions? options = null)
    {
        using var writer = new StringWriter();
        Serialize(items, writer, options);
        return writer.ToString();
    }

    public static void Serialize(IEnumerable<SrtItem> items, TextWriter writer, SrtSerializerOptions? options = null)
    {
        options ??= new SrtSerializerOptions();
        int currentIndex = 1;
        Span<char> timeBuffer = stackalloc char[12];

        foreach (var item in items)
        {
            int index = options.Renumber ? currentIndex++ : item.Index;
            TimeSpan start = item.StartTime + options.TimeOffset;
            TimeSpan end = item.EndTime + options.TimeOffset;

            writer.Write(index.ToString(CultureInfo.InvariantCulture));
            writer.Write(options.NewLine);

            FormatTimeSpan(start, timeBuffer);
            writer.Write(timeBuffer);
            writer.Write(" --> ");

            FormatTimeSpan(end, timeBuffer);
            writer.Write(timeBuffer);
            writer.Write(options.NewLine);

            writer.Write(item.Text);
            writer.Write(options.NewLine);
            writer.Write(options.NewLine);
        }
    }

    public static async Task SerializeAsync(
        IEnumerable<SrtItem> items,
        TextWriter writer,
        SrtSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        options ??= new SrtSerializerOptions();
        int currentIndex = 1;
        Memory<char> timeBuffer = new char[12];

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int index = options.Renumber ? currentIndex++ : item.Index;
            TimeSpan start = item.StartTime + options.TimeOffset;
            TimeSpan end = item.EndTime + options.TimeOffset;

            await writer.WriteAsync(index.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
            await writer.WriteAsync(options.NewLine).ConfigureAwait(false);

            FormatTimeSpan(start, timeBuffer.Span);
            await writer.WriteAsync(timeBuffer).ConfigureAwait(false);
            await writer.WriteAsync(" --> ").ConfigureAwait(false);

            FormatTimeSpan(end, timeBuffer.Span);
            await writer.WriteAsync(timeBuffer).ConfigureAwait(false);
            await writer.WriteAsync(options.NewLine).ConfigureAwait(false);

            await writer.WriteAsync(item.Text).ConfigureAwait(false);
            await writer.WriteAsync(options.NewLine).ConfigureAwait(false);
            await writer.WriteAsync(options.NewLine).ConfigureAwait(false);
        }
    }

    private static void FormatTimeSpan(TimeSpan ts, Span<char> buffer)
    {
        if (ts < TimeSpan.Zero)
            ts = TimeSpan.Zero;

        int totalHours = (int)ts.TotalHours;
        int minutes = ts.Minutes;
        int seconds = ts.Seconds;
        int millis = ts.Milliseconds;

        buffer[0] = (char)('0' + (Math.Min(totalHours, 99) / 10));
        buffer[1] = (char)('0' + (Math.Min(totalHours, 99) % 10));
        buffer[2] = ':';
        buffer[3] = (char)('0' + (minutes / 10));
        buffer[4] = (char)('0' + (minutes % 10));
        buffer[5] = ':';
        buffer[6] = (char)('0' + (seconds / 10));
        buffer[7] = (char)('0' + (seconds % 10));
        buffer[8] = ',';
        buffer[9] = (char)('0' + (millis / 100));
        buffer[10] = (char)('0' + ((millis / 10) % 10));
        buffer[11] = (char)('0' + (millis % 10));
    }
}
