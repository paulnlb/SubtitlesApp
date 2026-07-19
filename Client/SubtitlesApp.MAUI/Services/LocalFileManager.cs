using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public partial class LocalFileManager
{
    public partial Task<IFileResource?> PickFileAsync(string[] mimeTypes);

    public partial Result<Stream> GetFileStream(string uriString);
}
