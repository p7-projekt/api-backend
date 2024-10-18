using Core.Sessions.Models;
using FluentValidation;

namespace Core.Sessions.Validators;

public class CreateSessionDtoValidator : AbstractValidator<CreateSessionDto>
{
    public CreateSessionDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.Title).MaximumLength(100).WithMessage("Maximum 100 characters in the title");
        RuleFor(x => x.Title).Must(IsLetterOrDigit).WithMessage("Title can only consist of letters and digits");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null)
            .WithMessage("Description can maximally be 1000 charachters");
        RuleFor(x => x.ExpiresInHours).NotEmpty().WithMessage("Number of hours for the session is required");
        RuleFor(x => x.ExpiresInHours).GreaterThanOrEqualTo(1).LessThanOrEqualTo(10).WithMessage("The session can only be between 1 hour and 10 hours");
        RuleFor(x => x.ExerciseIds).NotEmpty().WithMessage("ExerciseIds must contain at least one item.");
;    }

    private bool IsLetterOrDigit(string sentence)
    {
        return sentence.All(char.IsAsciiLetterOrDigit);
    }
}