using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Infrastructure.Interfaces;

public interface ICustomFilePicker
{
    Task<string?> PickAsync(string[] mimeTypes);

    Result<string> GetFileName(string uriString);

    Result<Stream> GetFileStream(string uriString);

    Result<IFileResource> GetLocalPath(Uri uri);
}
