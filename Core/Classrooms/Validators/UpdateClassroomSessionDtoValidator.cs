using Core.Classrooms.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Validators;

public class UpdateClassroomSessionDtoValidator : AbstractValidator<UpdateClassroomSessionDto>
{
    public UpdateClassroomSessionDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Session id must be provided");
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Classroom id must be greater than 0");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Session must have a title");
        RuleFor(x => x.Title).MaximumLength(100).WithMessage("Session title must be no longer than 100 characters");
        RuleFor(x => x.Active).Must(value => value == true || value == false).WithMessage("Active level of session must be provided");
        RuleFor(x => x.ExerciseIds).NotEmpty().WithMessage("Exercises must be provided for session");
        RuleFor(x => x.ExerciseIds).ForEach(y => y.GreaterThan(0).WithMessage("Exercise ids must be greater than 0"));
        RuleFor(x => x.LanguageIds).NotEmpty().WithMessage("Language must be provided for session");
        RuleFor(x => x.LanguageIds).ForEach(y => y.GreaterThan(0).WithMessage("Language ids must be greater than 0"));
    }
}
