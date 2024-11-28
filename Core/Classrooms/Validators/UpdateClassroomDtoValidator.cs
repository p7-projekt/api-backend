using Core.Classrooms.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Validators;

public class UpdateClassroomDtoValidator : AbstractValidator<UpdateClassroomDto>
{
    public UpdateClassroomDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Classrooms must have a title");
        RuleFor(x => x.Title).MaximumLength(100).WithMessage("Title must be no longer than 100 characters");
        RuleFor(x => x.RegistrationOpen).(value => value == true || value == false).WithMessage("Registratin level of classroom must be set");
        RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Max 1000 characters for the description");
    }
}
