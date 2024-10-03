using Microsoft.AspNetCore.Mvc;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WhisperController(
    ILogger<WhisperController> logger,
    ITranscriptionService transcriptionService,
    IWaveService waveService) : ControllerBase
{
    [HttpPost("transcription")]
    public async Task<IActionResult> TranscribeAudio(
        IFormFile audioFile,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Connected");

        using var audioStream = audioFile.OpenReadStream();
        using var binaryReader = new BinaryReader(audioStream);
        var audioBytes = binaryReader.ReadBytes((int)audioStream.Length);

        var validationResult = waveService.ValidateAudio(audioBytes);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var subtitles = await transcriptionService.TranscribeAudioAsync(
            audioBytes,
            cancellationToken);

        logger.LogInformation("Transcribing done.");

        return Ok(subtitles);
    }
}
