using System.Globalization;
using Com.Arthenica.Ffmpegkit;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessingService
{
    public async partial Task<Stream> ExtractAudioAsync(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        int audioTrackIndex,
        CancellationToken cancellationToken
    )
    {
        var inputPath = PrepareInputPath(mediaPath);
        var output = FFmpegKitOutputBuffer.Create(AudioFormats.Wave);

        var ffmpegSession = await ExecuteCommandAsync(
            $"-ss {startTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
                + $"-to {endTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
                + $"-i '{inputPath}' "
                + $"-map 0:a:{audioTrackIndex} "
                + $"-ar 16000 "
                + $"-ac 1 "
                + $"-f {AudioFormats.Wave} "
                + output.Url
        );

        if (ffmpegSession.ReturnCode?.IsValueError == true)
        {
            output.Close();
            throw new InvalidOperationException($"FFmpeg extraction failed with error {GetErrorLogs(ffmpegSession)}.");
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return memoryStream;
    }

    public async partial Task<Stream> CopySubtitlesAsync(
        string mediaPath,
        string format,
        int subtitleTrackIndex,
        CancellationToken cancellationToken
    )
    {
        var inputPath = PrepareInputPath(mediaPath);
        var output = FFmpegKitOutputBuffer.Create(format);

        var ffmpegSession = await ExecuteCommandAsync(
            $"-i '{inputPath}' -map 0:s:{subtitleTrackIndex} -c copy {output.Url}"
        );

        if (ffmpegSession.ReturnCode?.IsValueError == true)
        {
            output.Close();
            throw new InvalidOperationException($"FFmpeg extraction failed with error {GetErrorLogs(ffmpegSession)}.");
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return memoryStream;
    }

    public async partial Task<Stream> ExtractSubtitlesAsync(
        string mediaPath,
        string outputFormat,
        int subtitleTrackIndex,
        CancellationToken cancellationToken
    )
    {
        var inputPath = PrepareInputPath(mediaPath);
        var output = FFmpegKitOutputBuffer.Create(outputFormat);

        var ffmpegSession = await ExecuteCommandAsync($"-i '{inputPath}' -map 0:s:{subtitleTrackIndex} {output.Url}");

        if (ffmpegSession.ReturnCode?.IsValueError == true)
        {
            output.Close();
            throw new InvalidOperationException($"FFmpeg extraction failed with error {GetErrorLogs(ffmpegSession)}.");
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return memoryStream;
    }

    private string PrepareInputPath(string mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            throw new InvalidOperationException("Source media path is not set");
        }

        if (!IsRemoteUrl(mediaPath))
        {
            var uri = Android.Net.Uri.Parse(mediaPath);
            return FFmpegKitConfig.GetSafParameterForRead(Platform.CurrentActivity, uri);
        }

        return mediaPath;
    }

    private Task<FFmpegSession> ExecuteCommandAsync(string command)
    {
        var tcs = new TaskCompletionSource<FFmpegSession>();
        var callback = new FfmpegCallback(tcs);

        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        FFmpegKit.ExecuteAsync(command, callback);

        return tcs.Task;
    }

    private string GetErrorLogs(FFmpegSession session)
    {
        return string.Join(Environment.NewLine, session.Logs.Where(x => x.Level == Level.AvLogError).Select(x => x.Message));
    }

    private bool IsRemoteUrl(string path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uriResult);
        return uriCreated && (uriResult!.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class FfmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
{
    private readonly TaskCompletionSource<FFmpegSession> _tcs;

    public FfmpegCallback(TaskCompletionSource<FFmpegSession> tcs)
    {
        _tcs = tcs;
    }

    public void Apply(FFmpegSession? p0)
    {
        if (p0?.ReturnCode is null)
        {
            _tcs.SetException(new ArgumentNullException());
            return;
        }
        _tcs.SetResult(p0);
    }
}
