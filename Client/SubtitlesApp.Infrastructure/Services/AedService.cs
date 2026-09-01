using Microsoft.Extensions.Logging;
using NAudio.Wave;
using OmniVadDotnet.Android;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Helpers;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Services;

public partial class AedService(ILogger<AedService> logger)
{
    public AedSegments Detect(Stream audio, TimeSpan minNoVoiceLength = default)
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

        config.Music.MinSpeechFrames = config.Speech.MinSpeechFrames = config.Singing.MinSpeechFrames = 60;
        config.Music.MaxSpeechFrames = config.Speech.MaxSpeechFrames = config.Singing.MaxSpeechFrames = 2000;
        config.Music.ExtendSpeechFrames = config.Speech.ExtendSpeechFrames = config.Singing.ExtendSpeechFrames = 50;

        var allSamplesArr = allSamples.ToArray();
        MediaHelper.PeakNormalize(allSamplesArr);

        var segments = OmniVad.AedDetect(allSamplesArr, modelPath, config);

        var voiceSegments = segments
            .Where(s => s.Cls == OmniAedClass.Singing || s.Cls == OmniAedClass.Speech)
            .Select(s => new TimeInterval(TimeSpan.FromSeconds(s.Start), TimeSpan.FromSeconds(s.End)));

        var timeSet = new TimeSet(voiceSegments, minNoVoiceLength);

        var result = new AedSegments
        {
            VoiceSegments = timeSet.GetAllIntervals().ToList(),
            NonVoiceSegments = timeSet.GetAllGapsInside(TimeSpan.Zero, waveReader.TotalTime).ToList(),
        };

        LogAedSegments(result);

        return result;
    }

    private void LogAedSegments(AedSegments aedResult)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        foreach (var segment in aedResult.VoiceSegments)
        {
            LogVoiceSegment(segment.StartTime, segment.EndTime);
        }

        foreach (var segment in aedResult.NonVoiceSegments)
        {
            LogNonVoiceSegment(segment.StartTime, segment.EndTime);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Detected voice. St: {StartTime}, Et: {EndTime}")]
    private partial void LogVoiceSegment(TimeSpan startTime, TimeSpan endTime);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Detected voice absence. St: {StartTime}, Et: {EndTime}")]
    private partial void LogNonVoiceSegment(TimeSpan startTime, TimeSpan endTime);
}
