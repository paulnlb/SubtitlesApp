using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Infrastructure.Constants;

namespace SubtitlesServer.WhisperApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimiterConstants.WhisperPolicy)]
[Authorize]
public class WhisperController(
    ILogger<WhisperController> logger,
    ITranscriptionService transcriptionService,
    IWaveService waveService,
    LanguageService languageService
) : ControllerBase
{
    [HttpPost("transcription")]
    public async Task<IActionResult> TranscribeAudio(
        IFormFile audioFile,
        [FromHeader(Name = RequestConstants.SubtitlesLanguageHeader)] string subtitlesLanguageCode,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Connected");

        var subtitlesLaguage = languageService.GetLanguageByCode(subtitlesLanguageCode);

        if (subtitlesLaguage == null)
        {
            logger.LogError("Invalid language code: {languageCode}", subtitlesLanguageCode);
            return BadRequest("Invalid language code.");
        }

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
            subtitlesLanguageCode,
            cancellationToken
        );

        logger.LogInformation("Transcribing done.");

        return Ok(subtitles);
    }
}
