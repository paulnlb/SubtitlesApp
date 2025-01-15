using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Shared.Extensions;
using SubtitlesServer.TranslationApi.Services.Interfaces;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TranslationController(ITranslationService translationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var result = await translationService.TranslateAsync(request);

        return this.ConvertToActionResult(result);
    }

    [HttpPost("stream")]
    public IActionResult TranslateAndStream([FromBody] TranslationRequestDto request)
    {
        var translationResult = translationService.TranslateAndStreamAsync(request);

        return this.ConvertToActionResult(translationResult);
    }
}
