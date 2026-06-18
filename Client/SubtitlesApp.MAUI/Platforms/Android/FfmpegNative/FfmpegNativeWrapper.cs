using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SubtitlesApp.Platforms.Android.FfmpegNative;

public static partial class FfmpegNativeWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int WriteCallback(IntPtr opaque, IntPtr buf, int buf_size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long SeekCallback(IntPtr opaque, long offset, int whence);

    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [LibraryImport("ffmpegwrapper", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int extract_audio(
        double start_time_sec,
        double end_time_sec,
        string source_path,
        int sample_rate,
        string output_format,
        IntPtr opaque,
        WriteCallback write_cb,
        SeekCallback? seek_cb
    );

    private const int SEEK_SET = 0;
    private const int SEEK_CUR = 1;
    private const int SEEK_END = 2;
    private const int AVSEEK_SIZE = 0x10000;
    private const int AVSEEK_FORCE = 0x20000;

    public static int ExtractToStream(
        string sourcePath,
        Stream outputStream,
        double startSec,
        double endSec,
        int sampleRate,
        string format
    )
    {
        if (outputStream == null)
            throw new ArgumentNullException(nameof(outputStream));
        if (!outputStream.CanWrite)
            throw new ArgumentException("Output stream must be writable.");

        byte[] managedBuf = new byte[32768];

        WriteCallback writeCb = (opaque, buf, buf_size) =>
        {
            try
            {
                if (managedBuf.Length < buf_size)
                    managedBuf = new byte[buf_size];

                Marshal.Copy(buf, managedBuf, 0, buf_size);
                outputStream.Write(managedBuf, 0, buf_size);
                return buf_size;
            }
            catch (Exception)
            {
                return -1;
            }
        };

        SeekCallback seekCb = (opaque, offset, whence) =>
        {
            try
            {
                // FFmpeg requests the size of the stream without changing position
                if (whence == AVSEEK_SIZE)
                    return outputStream.CanSeek ? outputStream.Length : -1;

                // Strip internal FFmpeg flags (like AVSEEK_FORCE) to get the base seek origin
                int originFlag = whence & ~AVSEEK_FORCE;
                SeekOrigin origin;

                switch (originFlag)
                {
                    case SEEK_SET:
                        origin = SeekOrigin.Begin;
                        break;
                    case SEEK_CUR:
                        origin = SeekOrigin.Current;
                        break;
                    case SEEK_END:
                        origin = SeekOrigin.End;
                        break;
                    default:
                        return -1;
                }

                return outputStream.Seek(offset, origin);
            }
            catch (Exception)
            {
                return -1;
            }
        };

        // The opaque parameter (IntPtr.Zero here) is available if you need to identify context
        // when multiplexing streams across multiple threads.
        int result = extract_audio(
            startSec,
            endSec,
            sourcePath,
            sampleRate,
            format,
            IntPtr.Zero,
            writeCb,
            outputStream.CanSeek ? seekCb : null // Only provide seekCb if the stream supports it
        );

        // Ensure delegates remain alive for the duration of the unmanaged call
        GC.KeepAlive(writeCb);
        GC.KeepAlive(seekCb);

        return result;
    }
}
