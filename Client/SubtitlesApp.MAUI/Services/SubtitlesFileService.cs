using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Services;
using SubtitlesApp.Mapper;

namespace SubtitlesApp.Services;

public class SubtitlesFileService(LocalFileManager fileManager)
{
    public async Task<ListResult<Subtitle>> ImportSrt()
    {
        var fileInfo = await fileManager.PickFile([
            "application/x-subrip", // Standard for Android 10 (API 29) and above
            "application/octet-stream", // Fallback for Android 9 and below
            "text/plain",
            "text/srt",
        ]);

        if (fileInfo is null)
        {
            return ListResult<Subtitle>.Failure(new Error(ErrorCode.OperationCancelled));
        }

        var streamResult = fileManager.GetFileStream(fileInfo.Uri);

        if (streamResult.IsFailure)
        {
            return ListResult<Subtitle>.Failure(streamResult.Error);
        }

        using var fileStream = streamResult.Value;
        using var fileReader = new StreamReader(fileStream);

        var srtSubs = SrtParser.ParseAsync(fileReader);
        var subtitles = new List<Subtitle>();

        await foreach (var sub in srtSubs)
        {
            subtitles.Add(SubtitlesMapper.ToSubtitle(sub));
        }

        return ListResult<Subtitle>.Success(subtitles);
    }

    public Task<Result> ExportSrt(IEnumerable<Subtitle> subtitles, string fileName = "exported_subtitles")
    {
        var srtItems = SubtitlesMapper.ToSrtItems(subtitles);
        var serialized = SrtSerializer.Serialize(srtItems);
        return fileManager.SaveTextFile(fileName + ".srt", serialized);
    }
}
