namespace SubtitlesApp.Infrastructure.Helpers;

public static class MediaHelper
{
    public static void PeakNormalize(Span<float> samples, float targetPeak = 0.891f)
    {
        float peak = 0f;

        foreach (float sample in samples)
        {
            peak = MathF.Max(peak, MathF.Abs(sample));
        }

        if (peak <= 0f)
        {
            return;
        }

        float gain = targetPeak / peak;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= gain;
        }
    }
}
