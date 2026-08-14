using System.Globalization;
using Com.Arthenica.Ffmpegkit;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegAudioExtractor : IAudioExtractor
{
    public async partial Task<Stream> ExtractAudioAsync(
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_sourcePath))
        {
            throw new InvalidOperationException("Source media path is not set");
        }

        var tcs = new TaskCompletionSource<int>();
        var callback = new FfmpegCallback(tcs);

        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        string inputPath;

        if (!IsRemoteUrl(_sourcePath))
        {
            var uri = Android.Net.Uri.Parse(_sourcePath);
            inputPath = FFmpegKitConfig.GetSafParameterForRead(Platform.CurrentActivity, uri);
        }
        else
        {
            inputPath = _sourcePath;
        }

        var output = FFmpegKitOutputBuffer.Create(AudioFormats.Wave);

        FFmpegKit.ExecuteAsync(
            $"-ss {startTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
                + $"-to {endTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
                + $"-i '{inputPath}' "
                + "-vn "
                + $"-ar 16000 "
                + $"-ac 1 "
                + "-y "
                + $"-f {AudioFormats.Wave} "
                + output.Url,
            callback
        );

        var exitCode = await tcs.Task;

        if (exitCode != 0)
        {
            output.Close();
            throw new InvalidOperationException($"FFmpeg extraction failed with exit code {exitCode}.");
        }

        var memoryStream = new MemoryStream(output.ToByteArray()) { Position = 0 };

        output.Close();

        return memoryStream;
    }

    private bool IsRemoteUrl(string path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uriResult);
        return uriCreated && (uriResult!.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class FfmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
{
    private readonly TaskCompletionSource<int> _tcs;

    public FfmpegCallback(TaskCompletionSource<int> tcs)
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

        var returnCode = p0.ReturnCode;
        _tcs.SetResult(returnCode.Value);
    }
}
