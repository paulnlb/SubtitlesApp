using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.TranslationApi.Extensions;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TranslationController(ITranslationService translationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var result = await translationService.TranslateAsync(request);

        return this.ConvertToActionResult(result);
    }
}
