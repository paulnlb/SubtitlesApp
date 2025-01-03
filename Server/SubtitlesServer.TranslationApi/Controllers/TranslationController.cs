using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.TranslationApi.Extensions;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TranslationController(
    ITranslationService translationService,
    LanguageService languageService,
    ILogger<TranslationController> logger
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var result = await translationService.TranslateAsync(request);

        return this.ConvertToActionResult(result);
    }
}
