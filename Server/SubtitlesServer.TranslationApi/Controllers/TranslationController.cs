using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TranslationController(
    ITranslationService translationService,
    LanguageService languageService,
    ILogger<TranslationController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var language = languageService.GetLanguageByCode(request.TargetLanguageCode);

        if (language == null)
        {
            const string message = "Invalid target language";
            logger.LogInformation(message);
            return BadRequest(message);
        }

        if (request.SourceSubtitles == null || !request.SourceSubtitles.Any())
        {
            const string message = "Source subtitles are required";
            logger.LogInformation(message);
            return BadRequest(message);
        }

        return Ok(await translationService.TranslateAsync(request));
    }
}
