using NAudio.Wave;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesServer.Application.Interfaces;
using System.Text;

namespace SubtitlesServer.Infrastructure.Services;

public class WaveService() : IWaveService
{
    public Result ValidateAudio(TrimmedAudioDto audioMetadata)
    {
        var audioBytes = audioMetadata.AudioBytes;
        var expectedChannelsCount = audioMetadata.ChannelsCount;
        var expectedSampleRate = audioMetadata.SampleRate;
        var invalidFormatError = new Error(ErrorCode.InvalidAudio, "Audio format is invalid or corrupted");

        // Minimum size for a valid WAV file (RIFF + fmt chunk headers)
        if (audioBytes == null || audioBytes.Length < 44)
        {
            return Result.Failure(invalidFormatError);
        }

        // Ensure the file starts with "RIFF" and "WAVE"
        if (Encoding.ASCII.GetString(audioBytes, 0, 4) != "RIFF" || Encoding.ASCII.GetString(audioBytes, 8, 4) != "WAVE")
        {
            return Result.Failure(invalidFormatError);
        }

        // Locate the "fmt " chunk (starts right after the RIFF header, at byte 12)
        int fmtChunkStart = 12;
        if (Encoding.ASCII.GetString(audioBytes, fmtChunkStart, 4) != "fmt ")
        {
            return Result.Failure(invalidFormatError);
        }

        // Read the audio format (PCM = 1)
        int audioFormat = BitConverter.ToInt16(audioBytes, fmtChunkStart + 8);
        if (audioFormat != 1)
        {
            return Result.Failure(invalidFormatError);
        }

        // Read the number of channels
        int channels = BitConverter.ToInt16(audioBytes, fmtChunkStart + 10);
        if (channels != expectedChannelsCount)
        {
            return Result.Failure(invalidFormatError);
        }

        // Read the sample rate
        int sampleRate = BitConverter.ToInt32(audioBytes, fmtChunkStart + 12);
        if (sampleRate != expectedSampleRate)
        {
            return Result.Failure(invalidFormatError);
        }

        // Find the "data" chunk (starts after the fmt chunk)
        int dataChunkStart = FindDataChunkStart(audioBytes);
        if (dataChunkStart == -1)
        {
            return Result.Failure(invalidFormatError);
        }

        // Read the number of bytes in the data chunk
        // ignore the chech if dataSize if it is -1 (unknown size)
        int dataSize = BitConverter.ToInt32(audioBytes, dataChunkStart + 4);
        if (dataSize != audioBytes.Length - dataChunkStart - 8
            && dataSize != -1)
        {
            return Result.Failure(invalidFormatError);
        }

        // Everything checks out
        return Result.Success();
    }

    // Helper method to find the "data" chunk (starts after fmt chunk)
    private static int FindDataChunkStart(byte[] audioBytes)
    {
        int position = 36; // Typically starts after the fmt chunk (which is 24 bytes from index 12)
        while (position < audioBytes.Length - 8)
        {
            // Look for the "data" chunk identifier
            if (Encoding.ASCII.GetString(audioBytes, position, 4) == "data")
            {
                return position;
            }
            // Move to the next chunk (skip over current chunk)
            int chunkSize = BitConverter.ToInt32(audioBytes, position + 4);
            position += 8 + chunkSize;
        }
        return -1; // Data chunk not found
    }
}
