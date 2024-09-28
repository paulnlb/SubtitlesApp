using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
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
        [FromBody] TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Connected");

        var validationResult = waveService.ValidateAudio(audioMetadata);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var subtitles = await transcriptionService.TranscribeAudioAsync(
            audioMetadata,
            cancellationToken);

        logger.LogInformation("Transcribing done.");

        return Ok(subtitles);
    }
}
