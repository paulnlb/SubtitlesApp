using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FilePicker : ICustomFilePicker
{
    public partial Task<string?> PickAsync(string[] mimeTypes);

    public partial Result<string> GetFileName(string uriString);

    public partial Result<Stream> GetFileStream(string uriString);

    public partial Result<IFileResource> GetLocalPath(Uri uri);
}
