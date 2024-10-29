using System.Data;
using Core.Solutions.Models;
using FluentValidation;

namespace Core.Solutions.Validators;

public class SubmitSolutionDtoValidator : AbstractValidator<SubmitSolutionDto>
{
    public SubmitSolutionDtoValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty().WithMessage(x => $"{nameof(x.ExerciseId)} cannot be empty");
        RuleFor(x => x.Solution).NotEmpty().WithMessage(x => $"{nameof(x.Solution)} cannot be empty");
    }
}