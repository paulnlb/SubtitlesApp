using FluentValidation;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Validators;

public class WhisperRequestModelValidator : AbstractValidator<WhisperRequestModel>
{
    public WhisperRequestModelValidator()
    {
        RuleFor(requestModel => requestModel.AudioFile)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Missing audio file");
        RuleFor(requestModel => requestModel.MaxSegmentLength).GreaterThanOrEqualTo(0);
        RuleFor(requestModel => requestModel.LanguageCode)
            .Must(x => x != null && x.Length > 0 && x.Length <= 4)
            .WithMessage("Language code is invalid");
    }
}
