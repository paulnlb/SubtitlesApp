using Microsoft.Extensions.Logging;
using NAudio.Wave;
using OmniVadDotnet.Android;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Helpers;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Services;

public partial class AedService(ILogger<AedService> logger)
{
    public Result<AedSegments> Detect(Stream audio, TimeSpan minNoVoiceLength = default)
    {
        var modelPath = AssetsHelper.ExtractAssetToAppData("Resources/Models/aed.omnivad", "aed.omnivad", "models");

        using var waveReader = new WaveFileReader(audio);
        var sampleProvider = waveReader.ToSampleProvider();

        var allSamples = new List<float>();

        Span<float> buffer = stackalloc float[4096];
        int samplesRead;

        while ((samplesRead = sampleProvider.Read(buffer)) > 0)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                allSamples.Add(buffer[i]);
            }
        }

        var config = OmniVad.GetDefaultAedConfig();

        config.Speech.Threshold = 0.4f;

        config.Singing.MinSpeechFrames = 60;
        config.Music.ExtendSpeechFrames = config.Speech.ExtendSpeechFrames = config.Singing.ExtendSpeechFrames = 50;

        var allSamplesArr = allSamples.ToArray();
        MediaHelper.PeakNormalize(allSamplesArr);

        List<OmniAedSegment> segments;

        try
        {
            segments = OmniVad.AedDetect(allSamplesArr, modelPath, config);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                LogAedSegments(segments);
            }
        }
        catch (Exception ex)
        {
            return Result<AedSegments>.Failure(new Error(ErrorCode.InternalClientError, ex.Message));
        }

        var voiceSegments = segments
            .Where(s => s.Cls == OmniAedClass.Singing || s.Cls == OmniAedClass.Speech)
            .Select(s => new TimeInterval(TimeSpan.FromSeconds(s.Start), TimeSpan.FromSeconds(s.End)));

        var timeSet = new TimeSet(voiceSegments, minNoVoiceLength);

        var aedSegments = new AedSegments
        {
            VoiceSegments = timeSet.GetAllIntervals().ToList(),
            NonVoiceSegments = timeSet.GetAllGapsInside(TimeSpan.Zero, waveReader.TotalTime).ToList(),
        };

        return Result<AedSegments>.Success(aedSegments);
    }

    private void LogAedSegments(List<OmniAedSegment> segments)
    {
        foreach (var segment in segments)
        {
            LogSegment(
                segment.Cls,
                TimeSpan.FromSeconds(segment.Start),
                TimeSpan.FromSeconds(segment.End),
                segment.Confidence
            );
        }
    }

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Omnivad AED segments: Cls: {Cls}, St: {StartTime}, Et: {EndTime}, Confidence: {Confidence}"
    )]
    private partial void LogSegment(OmniAedClass cls, TimeSpan startTime, TimeSpan endTime, float confidence);
}
