using Android.OS;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Platforms.Android;

public class AndroidFileResource(
    ParcelFileDescriptor descriptor,
    FileResourceType type,
    string id,
    string name,
    string uri
) : IFileResource
{
    public FileResourceType Type => type;
    public string Id => id;
    public string Name => name;
    public string Uri => uri;
    public string? AbsolutePath => $"/proc/self/fd/{descriptor.Fd}";

    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                descriptor.Dispose();
            }

            _disposed = true;
        }
    }
}
