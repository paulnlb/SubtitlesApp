using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Models;
using SubtitlesServer.WhisperApi.Services.Interfaces;

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
        [FromForm] WhisperRequestModel requestModel,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Connected");

        var subtitlesLaguage = languageService.GetLanguageByCode(requestModel.LanguageCode);

        if (subtitlesLaguage == null)
        {
            logger.LogError("Invalid language code: {languageCode}", requestModel.LanguageCode);
            return BadRequest(new Error(ErrorCode.BadRequest, "Invalid language code"));
        }

        var validationResult = waveService.ValidateAudio(requestModel.AudioFile);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var subtitles = await transcriptionService.TranscribeAudioAsync(requestModel, cancellationToken);

        logger.LogInformation("Transcribing done.");

        logger.LogInformation(JsonSerializer.Serialize(subtitles));

        return Ok(subtitles);
    }
}
