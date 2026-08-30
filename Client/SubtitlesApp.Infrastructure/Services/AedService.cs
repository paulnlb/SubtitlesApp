using NAudio.Wave;
using OmniVadDotnet.Android;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Services;

public class AedService
{
    public AedResult Detect(Stream audio)
    {
        var modelPath = AssetsHelper.ExtractAssetToAppData("models/aed.omnivad", "aed.omnivad", "models");

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

        if (audio.CanSeek)
        {
            audio.Position = 0;
        }

        var segments = OmniVad.AedDetect(allSamples.ToArray(), modelPath);

        var voiceSegments = segments
            .Where(s => s.Cls == OmniAedClass.Singing || s.Cls == OmniAedClass.Speech)
            .OrderBy(s => s.Start)
            .Select(s => new TimeInterval(TimeSpan.FromSeconds(s.Start), TimeSpan.FromSeconds(s.End)));

        var nonVoiceSegments = new TimeInterval(TimeSpan.Zero, waveReader.TotalTime).GetGapsBetween(voiceSegments, true);

        return new AedResult { VoiceSegments = voiceSegments.ToList(), NonVoiceSegments = nonVoiceSegments.ToList() };
    }
}
