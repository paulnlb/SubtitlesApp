using System.Runtime.InteropServices;

namespace OmniVadDotnet.Android;

public static unsafe class OmniVad
{
    public static List<OmniAedSegment> AedDetect(float[] samples, string bundlePath, OmniAedPostConfig? config = null)
    {
        var resultSegments = new List<OmniAedSegment>();

        using var aed = OmniVadNative.omni_aed_create(bundlePath, out int err);

        if (aed.IsInvalid)
        {
            string errMsg = Marshal.PtrToStringUTF8(OmniVadNative.omni_error_string(err)) ?? "Unknown error";
            throw new InvalidOperationException($"Failed to create AED: {errMsg}");
        }

        var aedConfig = config == null ? OmniVadNative.omni_aed_post_config_default() : config.Value;

        var ret = OmniVadNative.omni_aed_detect(
            aed,
            samples,
            samples.Length,
            in aedConfig,
            out OmniAedSegment* segments,
            out int count
        );

        if (ret != (int)OmniErrorCode.Ok)
        {
            string errMsg = Marshal.PtrToStringUTF8(OmniVadNative.omni_error_string(ret)) ?? "Unknown error";
            throw new InvalidOperationException(errMsg);
        }

        for (int i = 0; i < count; i++)
        {
            var seg = segments[i];
            resultSegments.Add(
                new()
                {
                    Start = seg.Start,
                    End = seg.End,
                    Cls = seg.Cls,
                    Confidence = seg.Confidence,
                }
            );
        }

        if (segments != null)
        {
            OmniVadNative.omni_free(segments);
        }

        return resultSegments;
    }
}
