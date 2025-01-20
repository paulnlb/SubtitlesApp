using FluentValidation;
using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.TranslationApi.Validators;

public class TranslationRequestDtoValidator : AbstractValidator<TranslationRequestDto>
{
    public TranslationRequestDtoValidator()
    {
        RuleFor(dto => dto.SourceSubtitles).NotEmpty().WithMessage("Provide at least one subtitle to translate");
        RuleFor(dto => dto.TargetLanguageCode)
            .Must(x => x != null && x.Length > 0 && x.Length <= 4)
            .WithMessage("Language code is invalid");
    }
}
