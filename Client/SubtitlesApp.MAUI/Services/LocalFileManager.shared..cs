using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Services;

public partial class LocalFileManager
{
    public partial Task<MediaFileInfo?> PickFile(string[] mimeTypes);

    public partial Result<Stream> GetFileStream(string uriString);
}
