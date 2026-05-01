using Microsoft.AspNetCore.Mvc;
using SubtitlesServer.Shared.Models;
using SubtitlesServer.Shared.Result;
using SubtitlesServer.TranslationApi.Interfaces;

namespace SubtitlesServer.TranslationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TranslationController(ITranslationService translationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
    {
        var result = await translationService.TranslateAsync(request);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.BadRequest => BadRequest(result.Error),
                ErrorCode.BadGateway => StatusCode(statusCode: StatusCodes.Status502BadGateway, result.Error),
                _ => StatusCode(statusCode: StatusCodes.Status500InternalServerError, result.Error),
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("stream")]
    public IActionResult TranslateAndStream([FromBody] TranslationRequestDto request)
    {
        var result = translationService.TranslateAndStreamAsync(request);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.BadRequest => BadRequest(result.Error),
                ErrorCode.BadGateway => StatusCode(statusCode: StatusCodes.Status502BadGateway, result.Error),
                _ => StatusCode(statusCode: StatusCodes.Status500InternalServerError, result.Error),
            };
        }

        return Ok(result.Value);
    }
}
