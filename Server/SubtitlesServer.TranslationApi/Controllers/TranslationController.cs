using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TranslationController(
    ITranslationService translationService,
    LanguageService languageService) : ControllerBase
{
    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var language = languageService.GetLanguageByCode(request.TargetLanguageCode);

        if (language == null)
        {
            return BadRequest("Invalid target language");
        }

        if (request.SourceSubtitles == null || !request.SourceSubtitles.Any())
        {
            return BadRequest("Source subtitles are required");
        }

        return Ok(await translationService.TranslateAsync(request));
    }
}
