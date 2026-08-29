using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

namespace OmniVadDotnet.Android;

#region Enums

public enum OmniErrorCode : int
{
    Ok = 0,
    ErrNullHandle = -1,
    ErrNullPointer = -2,
    ErrLoadBundle = -3,
    ErrLoadParam = -4,
    ErrLoadModel = -5,
    ErrLoadCmvn = -6,
    ErrNoFrames = -7,
    ErrInference = -8,
    ErrOutOfMemory = -9,
    ErrInvalidArg = -10,
}

public enum OmniAedClass : int
{
    Speech = 0,
    Singing = 1,
    Music = 2,
}

public enum OmniAedEventKind : int
{
    Silence = 0,
    Speech = 1,
    Singing = 2,
    Music = 3,
    Mixed = 4,
}

public enum OmniChunkMode : int
{
    Greedy = 0,
    LongestGap = 1,
}

#endregion

#region Structs

[StructLayout(LayoutKind.Sequential)]
public struct OmniSegment
{
    public float Start;
    public float End;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniAedSegment
{
    public float Start;
    public float End;
    public OmniAedClass Cls;
    public float Confidence;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniPostConfig
{
    public float Threshold;
    public int SmoothWindowSize;
    public int MinSpeechFrames;
    public int MinSilenceFrames;
    public int MaxSpeechFrames;
    public int MergeSilenceFrames;
    public int ExtendSpeechFrames;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniStreamVadConfig
{
    public float Threshold;
    public int SmoothWindowSize;
    public int PadStartFrame;
    public int MinSpeechFrame;
    public int MaxSpeechFrame;
    public int MinSilenceFrame;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniStreamVadResult
{
    public float Confidence;
    public float SmoothedProb;

    [MarshalAs(UnmanagedType.I1)]
    public bool IsSpeech;

    [MarshalAs(UnmanagedType.I1)]
    public bool IsSpeechStart;

    [MarshalAs(UnmanagedType.I1)]
    public bool IsSpeechEnd;
    public int FrameIdx;
    public int SpeechStartFrame;
    public int SpeechEndFrame;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniAedPostConfig
{
    public OmniPostConfig Speech;
    public OmniPostConfig Singing;
    public OmniPostConfig Music;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniAedOverlapConfig
{
    public int HopMs;
    public int OverlapMs;
    public int EdgeGuardMs;
    public int HardSplitPauseMs;
    public int MaxChunkMs;
    public int MinSpeechMs;
    public int MergeGapMs;
    public int MusicGapToleranceMs;
    public int PadStartMs;
    public int PadEndMs;
    public float SpeechThreshold;
    public float SingingThreshold;
    public float MusicThreshold;
    public int HardSplitLookaheadMs;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniAedOnlineEvent
{
    public float Start;
    public float End;
    public OmniAedEventKind PrimaryKind;
    public uint KindMask;
    public float SpeechConfidence;
    public float SingingConfidence;
    public float MusicConfidence;
    public float Confidence;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniAedOnlineSegment
{
    public float Start;
    public float End;
    public int EventStartIdx;
    public int EventCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniChunk
{
    public float Start;
    public float End;
    public int SegStartIdx;
    public int SegCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct OmniChunkConfig
{
    public float MaxChunkSecs;
    public float MaxGapSecs;
    public float PadOnsetSecs;
    public float PadOffsetSecs;
    public float MinSpeechSecs;
    public float MinSilenceSecs;
    public OmniChunkMode Mode;
}

#endregion

#region Safe Handles

public sealed class OmniVadHandle : SafeHandle
{
    public OmniVadHandle()
        : base(IntPtr.Zero, true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        OmniVadNative.omni_vad_destroy(handle);
        return true;
    }
}

public sealed class OmniStreamVadHandle : SafeHandle
{
    public OmniStreamVadHandle()
        : base(IntPtr.Zero, true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        OmniVadNative.omni_stream_vad_destroy(handle);
        return true;
    }
}

public sealed class OmniAedHandle : SafeHandle
{
    public OmniAedHandle()
        : base(IntPtr.Zero, true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        OmniVadNative.omni_aed_destroy(handle);
        return true;
    }
}

public sealed class OmniAedOverlapSegmenterHandle : SafeHandle
{
    public OmniAedOverlapSegmenterHandle()
        : base(IntPtr.Zero, true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        OmniVadNative.omni_aed_overlap_segmenter_destroy(handle);
        return true;
    }
}

#endregion

#region Native Imports

internal static unsafe partial class OmniVadNative
{
    private const string LibName = "omnivad";

    // --- Configuration Defaults ---
    [LibraryImport(LibName)]
    public static partial OmniPostConfig omni_post_config_default();

    [LibraryImport(LibName)]
    public static partial OmniStreamVadConfig omni_stream_vad_config_default();

    [LibraryImport(LibName)]
    public static partial OmniAedPostConfig omni_aed_post_config_default();

    [LibraryImport(LibName)]
    public static partial OmniAedOverlapConfig omni_aed_overlap_config_default();

    [LibraryImport(LibName)]
    public static partial OmniChunkConfig omni_chunk_config_default();

    // --- VAD ---
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial OmniVadHandle omni_vad_create(string bundle_path, out int out_error);

    [LibraryImport(LibName)]
    public static partial OmniVadHandle omni_vad_create_from_buffer(IntPtr data, int size, out int out_error);

    [LibraryImport(LibName)]
    public static partial int omni_vad_detect(
        OmniVadHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        in OmniPostConfig config,
        out OmniSegment* out_segments,
        out int out_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_vad_detect_int16(
        OmniVadHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        in OmniPostConfig config,
        out OmniSegment* out_segments,
        out int out_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_vad_detect_probs(
        OmniVadHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    public static partial int omni_vad_detect_probs_int16(
        OmniVadHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    internal static partial void omni_vad_destroy(IntPtr handle);

    // --- Stream VAD ---
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial OmniStreamVadHandle omni_stream_vad_create(
        string bundle_path,
        in OmniStreamVadConfig config,
        out int out_error
    );

    [LibraryImport(LibName)]
    public static partial OmniStreamVadHandle omni_stream_vad_create_from_buffer(
        IntPtr data,
        int size,
        in OmniStreamVadConfig config,
        out int out_error
    );

    [LibraryImport(LibName)]
    public static partial OmniStreamVadHandle omni_stream_vad_clone(OmniStreamVadHandle handle, out int out_error);

    [LibraryImport(LibName)]
    public static partial int omni_stream_vad_process(
        OmniStreamVadHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        out OmniStreamVadResult result
    );

    [LibraryImport(LibName)]
    public static partial int omni_stream_vad_process_int16(
        OmniStreamVadHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        out OmniStreamVadResult result
    );

    [LibraryImport(LibName)]
    public static partial int omni_stream_vad_detect_full(
        OmniStreamVadHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    public static partial int omni_stream_vad_detect_full_int16(
        OmniStreamVadHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    public static partial void omni_stream_vad_reset(OmniStreamVadHandle handle);

    [LibraryImport(LibName)]
    public static partial int omni_stream_vad_get_frame_offset(OmniStreamVadHandle handle);

    [LibraryImport(LibName)]
    internal static partial void omni_stream_vad_destroy(IntPtr handle);

    // --- AED ---
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial OmniAedHandle omni_aed_create(string bundle_path, out int out_error);

    [LibraryImport(LibName)]
    public static partial OmniAedHandle omni_aed_create_from_buffer(IntPtr data, int size, out int out_error);

    [LibraryImport(LibName)]
    public static partial int omni_aed_detect(
        OmniAedHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        in OmniAedPostConfig config,
        out OmniAedSegment* out_segments,
        out int out_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_detect_int16(
        OmniAedHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        in OmniAedPostConfig config,
        out OmniAedSegment* out_segments,
        out int out_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_detect_probs(
        OmniAedHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_detect_probs_int16(
        OmniAedHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        out float* out_probs,
        out int out_frames
    );

    [LibraryImport(LibName)]
    internal static partial void omni_aed_destroy(IntPtr handle);

    // --- AED Overlap Segmenter ---
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial OmniAedOverlapSegmenterHandle omni_aed_overlap_segmenter_create(
        string bundle_path,
        in OmniAedOverlapConfig config,
        out int out_error
    );

    [LibraryImport(LibName)]
    public static partial OmniAedOverlapSegmenterHandle omni_aed_overlap_segmenter_create_from_buffer(
        IntPtr data,
        int size,
        in OmniAedOverlapConfig config,
        out int out_error
    );

    [LibraryImport(LibName)]
    public static partial OmniAedOverlapSegmenterHandle omni_aed_overlap_segmenter_clone(
        OmniAedOverlapSegmenterHandle handle,
        out int out_error
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_overlap_segmenter_ingest(
        OmniAedOverlapSegmenterHandle handle,
        ReadOnlySpan<float> audio_data,
        int num_samples,
        out OmniAedOnlineSegment* out_segments,
        out int out_segment_count,
        out OmniAedOnlineEvent* out_events,
        out int out_event_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_overlap_segmenter_ingest_int16(
        OmniAedOverlapSegmenterHandle handle,
        ReadOnlySpan<short> audio_data,
        int num_samples,
        out OmniAedOnlineSegment* out_segments,
        out int out_segment_count,
        out OmniAedOnlineEvent* out_events,
        out int out_event_count
    );

    [LibraryImport(LibName)]
    public static partial int omni_aed_overlap_segmenter_flush(
        OmniAedOverlapSegmenterHandle handle,
        out OmniAedOnlineSegment* out_segments,
        out int out_segment_count,
        out OmniAedOnlineEvent* out_events,
        out int out_event_count
    );

    [LibraryImport(LibName)]
    public static partial void omni_aed_overlap_segmenter_reset(OmniAedOverlapSegmenterHandle handle);

    [LibraryImport(LibName)]
    internal static partial void omni_aed_overlap_segmenter_destroy(IntPtr handle);

    // --- Chunking ---
    [LibraryImport(LibName)]
    public static partial int omni_merge_chunks(
        OmniSegment* segments,
        int num_segments,
        in OmniChunkConfig config,
        out OmniChunk* out_chunks,
        out int out_count
    );

    // --- Memory Management ---
    [LibraryImport(LibName)]
    public static partial void omni_free(void* ptr);

    [LibraryImport(LibName)]
    public static partial IntPtr omni_error_string(int error_code);
}

#endregion
