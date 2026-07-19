using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Services;

public partial class LocalFileManager
{
    public partial Task<MediaFileInfo?> PickFileAsync(string[] mimeTypes);

    public partial Result<Stream> GetFileStream(string uriString);
}
