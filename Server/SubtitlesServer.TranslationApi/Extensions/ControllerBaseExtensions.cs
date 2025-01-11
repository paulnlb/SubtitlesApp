using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.TranslationApi.Extensions;

public static class ControllerBaseExtensions
{
    /// <summary>
    /// Converts <see cref="Result"/> to <see cref="IActionResult"/> depending on <see cref="Result"/> status
    /// </summary>
    /// <param name="controllerBase"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IActionResult ConvertToActionResult(this ControllerBase controllerBase, Result result)
    {
        if (result.IsSuccess)
        {
            return controllerBase.Ok();
        }
        else
        {
            return ConvertFailedResultToActionResult(controllerBase, result);
        }
    }

    /// <summary>
    /// Converts <see cref="Result<typeparamref name="T"/>"/> to <see cref="IActionResult"/> depending on <see cref="Result<typeparamref name="T"/>"/> status
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controllerBase"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IActionResult ConvertToActionResult<T>(this ControllerBase controllerBase, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controllerBase.Ok(result.Value);
        }
        else
        {
            return ConvertFailedResultToActionResult(controllerBase, result);
        }
    }

    private static ObjectResult ConvertFailedResultToActionResult(this ControllerBase controllerBase, Result failedResult)
    {
        var error = failedResult.Error;

        return error.Code switch
        {
            ErrorCode.Unauthorized => controllerBase.Unauthorized(error),
            ErrorCode.BadRequest => controllerBase.BadRequest(error),
            ErrorCode.BadGateway => controllerBase.StatusCode(statusCode: StatusCodes.Status502BadGateway, error),
            _ => controllerBase.StatusCode(statusCode: StatusCodes.Status500InternalServerError, error),
        };
    }
}
