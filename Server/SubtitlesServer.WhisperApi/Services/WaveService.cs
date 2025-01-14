using System.Text;
using SubtitlesApp.Core.Result;
using SubtitlesServer.WhisperApi.Services.Interfaces;

namespace SubtitlesServer.WhisperApi.Services;

public class WaveService(ILogger<WaveService> logger) : IWaveService
{
    public Result ValidateAudio(IFormFile audio)
    {
        var expectedSampleRate = 16000;
        var invalidAudioError = new Error(ErrorCode.InvalidAudio, "Provided wave audio is invalid or corrupted");
        var invalidSampleRateError = new Error(
            ErrorCode.InvalidAudio,
            $"Provided wave audio has an invalid sample rate. Expected: {expectedSampleRate}"
        );

        using var audioStream = new MemoryStream();
        audio.CopyTo(audioStream);
        var audioBytes = audioStream.ToArray();

        // Minimum size for a valid WAV file (RIFF + fmt chunk headers)
        if (audioBytes == null || audioBytes.Length < 44)
        {
            logger.LogError("Audio file is too small: {size}", audioBytes?.Length);
            return Result.Failure(invalidAudioError);
        }

        // Ensure the file starts with "RIFF" and "WAVE"
        if (Encoding.ASCII.GetString(audioBytes, 0, 4) != "RIFF" || Encoding.ASCII.GetString(audioBytes, 8, 4) != "WAVE")
        {
            logger.LogError("Audio file does not start with RIFF/WAVE");
            return Result.Failure(invalidAudioError);
        }

        // Locate the "fmt " chunk (starts right after the RIFF header, at byte 12)
        int fmtChunkStart = 12;
        if (Encoding.ASCII.GetString(audioBytes, fmtChunkStart, 4) != "fmt ")
        {
            logger.LogError("Audio file does not contain a fmt chunk");
            return Result.Failure(invalidAudioError);
        }

        // Read the audio format (PCM = 1)
        int audioFormat = BitConverter.ToInt16(audioBytes, fmtChunkStart + 8);
        if (audioFormat != 1)
        {
            logger.LogError("Audio file is not in PCM format");
            return Result.Failure(invalidAudioError);
        }

        // Read the sample rate
        int sampleRate = BitConverter.ToInt32(audioBytes, fmtChunkStart + 12);
        if (sampleRate != expectedSampleRate)
        {
            logger.LogError("Audio file has an invalid sample rate: {sampleRate}", sampleRate);
            return Result.Failure(invalidSampleRateError);
        }

        // Find the "data" chunk (starts after the fmt chunk)
        int dataChunkStart = FindDataChunkStart(audioBytes);
        if (dataChunkStart == -1)
        {
            logger.LogError("Audio file does not contain a data chunk");
            return Result.Failure(invalidAudioError);
        }

        // Read the number of bytes in the data chunk
        // Ignore the check if dataSize equals to -1 (unknown size)
        int dataSize = BitConverter.ToInt32(audioBytes, dataChunkStart + 4);
        if (dataSize != audioBytes.Length - dataChunkStart - 8 && dataSize != -1)
        {
            logger.LogError("Audio file has an invalid data chunk size: {dataSize}", dataSize);
            return Result.Failure(invalidAudioError);
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
