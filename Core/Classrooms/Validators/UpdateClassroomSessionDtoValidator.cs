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
        RuleFor(x => x.Title).NotEmpty().WithMessage("Session must have a title");
        RuleFor(x => x.Title).MaximumLength(100).WithMessage("Session title must be no longer than 100 characters");
        RuleFor(x => x.Active).NotEmpty().WithMessage("Active level of session must be provided");
        RuleFor(x => x.ExerciseIds).NotEmpty().WithMessage("Exercises must be provided for session");
    }
}
