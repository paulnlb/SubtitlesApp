using Android.OS;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Platforms.Android;

public class FileDescriptorResource(ParcelFileDescriptor descriptor) : IFileResource
{
    public string Path => $"/proc/self/fd/{descriptor.Fd}";

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
