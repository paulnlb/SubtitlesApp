using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Shared.FluentValidation;

public class CustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(
        ActionExecutingContext context,
        ValidationProblemDetails? validationProblemDetails
    )
    {
        return new BadRequestObjectResult(
            new { Code = ErrorCode.ValidationFailed, Description = validationProblemDetails?.Errors }
        );
    }
}
