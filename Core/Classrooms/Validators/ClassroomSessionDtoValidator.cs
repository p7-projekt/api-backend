using Core.Classrooms.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Validators;

public class ClassroomSessionDtoValidator : AbstractValidator<ClassroomSessionDto>
{
    public ClassroomSessionDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Session must have a title");
        RuleFor(x => x.Title).MaximumLength(100).WithMessage("Title must be no longet than 100 characters");
        RuleFor(x => x.ExerciseIds).NotEmpty().WithMessage("Exercises must be associated to session");
        RuleFor(x => x.LanguageIds).NotEmpty().WithMessage("Allowed languages of session must be defined");
    }
}
