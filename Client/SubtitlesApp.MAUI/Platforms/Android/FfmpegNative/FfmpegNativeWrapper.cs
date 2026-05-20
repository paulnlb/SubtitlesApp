using System.Runtime.InteropServices;

namespace SubtitlesApp.Platforms.Android.FfmpegNative;

public static partial class FfmpegNativeWrapper
{
    [LibraryImport("ffmpegwrapper", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int extract_audio(
        double start_time_sec,
        double end_time_sec,
        string source_path,
        int sample_rate,
        string output_format,
        string output_path
    );
}
