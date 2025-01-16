using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimiterConstants.WhisperPolicy)]
public class WhisperController(
    ILogger<WhisperController> logger,
    ITranscriptionService transcriptionService,
    IAudioService waveService,
    LanguageService languageService
) : ControllerBase
{
    [HttpPost("transcription")]
    public async Task<IActionResult> TranscribeAudio(
        [FromForm] WhisperRequestModel requestModel,
        CancellationToken cancellationToken
    )
    {
        var subtitlesLanguage = languageService.GetLanguageByCode(requestModel.LanguageCode);

        if (subtitlesLanguage == null)
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

        return Ok(subtitles);
    }
}
