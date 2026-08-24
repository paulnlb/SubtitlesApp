using System.Globalization;
using Com.Arthenica.Ffmpegkit;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessingService
{
    public async partial Task<Result<Stream>> ExtractAudioAsync(
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
                + output.Url,
            cancellationToken
        );

        if (!IsSucceeded(ffmpegSession, out var error))
        {
            output.Close();
            return Result<Stream>.Failure(error!);
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return Result<Stream>.Success(memoryStream);
    }

    public async partial Task<Result<Stream>> CopySubtitlesAsync(
        string mediaPath,
        string format,
        int subtitleTrackIndex,
        CancellationToken cancellationToken
    )
    {
        var inputPath = PrepareInputPath(mediaPath);
        var output = FFmpegKitOutputBuffer.Create(format);

        var ffmpegSession = await ExecuteCommandAsync(
            $"-i '{inputPath}' -map 0:s:{subtitleTrackIndex} -c:s copy {output.Url}",
            cancellationToken
        );

        if (!IsSucceeded(ffmpegSession, out var error))
        {
            output.Close();
            return Result<Stream>.Failure(error!);
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return Result<Stream>.Success(memoryStream);
    }

    public async partial Task<Result<Stream>> ExtractSubtitlesAsync(
        string mediaPath,
        string outputFormat,
        int subtitleTrackIndex,
        CancellationToken cancellationToken
    )
    {
        var inputPath = PrepareInputPath(mediaPath);
        var output = FFmpegKitOutputBuffer.Create(outputFormat);

        var ffmpegSession = await ExecuteCommandAsync(
            $"-i '{inputPath}' -map 0:s:{subtitleTrackIndex} {output.Url}",
            cancellationToken
        );

        if (!IsSucceeded(ffmpegSession, out var error))
        {
            output.Close();
            return Result<Stream>.Failure(error!);
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return Result<Stream>.Success(memoryStream);
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

    private Task<FFmpegSession> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<FFmpegSession>();
        var callback = new FfmpegCallback(tcs);

        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        var session = FFmpegKit.ExecuteAsync(command, callback);

        cancellationToken.Register(() => FFmpegKit.Cancel(session.SessionId));

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

    private bool IsSucceeded(FFmpegSession ffmpegSession, out Error? error)
    {
        bool isSucceeded;

        if (ffmpegSession.ReturnCode?.IsValueSuccess is true)
        {
            error = null;
            isSucceeded = true;
        }
        else if (ffmpegSession.ReturnCode?.IsValueCancel is true)
        {
            error = new Error(ErrorCode.OperationCancelled);
            isSucceeded = false;
        }
        else if (ffmpegSession.ReturnCode?.IsValueError is true)
        {
            error = new Error(
                ErrorCode.MediaProcessingError,
                $"FFmpeg extraction failed with error:\n {GetErrorLogs(ffmpegSession)}."
            );
            isSucceeded = false;
        }
        else if (ffmpegSession.ReturnCode is null)
        {
            error = new Error(ErrorCode.MediaProcessingError, $"FfmpegKit session has no returning code");
            isSucceeded = false;
        }
        else
        {
            error = new Error(
                ErrorCode.MediaProcessingError,
                $"FfmpegKit returned an unknown return code. Value: {ffmpegSession.ReturnCode}"
            );
            isSucceeded = false;
        }

        return isSucceeded;
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
