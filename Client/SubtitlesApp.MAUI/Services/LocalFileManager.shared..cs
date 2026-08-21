using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Services;

public partial class LocalFileManager
{
    public partial Task<MediaFileInfo?> PickFile(string[] mimeTypes);

    public partial Result<Stream> GetFileStream(string uriString);

    public partial Task<Result> SaveTextFile(string fileName, string content);

    public partial Task<Result> SaveInternalTextFile(string outputFileName, string sourcePath);

    public partial Task<Result> SaveSrtFile(string fileName, string content);
}
