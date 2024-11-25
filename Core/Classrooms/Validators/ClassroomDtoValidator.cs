using Core.Classrooms.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Validators;

public class ClassroomDtoValidator : AbstractValidator<ClassroomDto>
{
    public ClassroomDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.Title).MinimumLength(1).MaximumLength(100).WithMessage("Max length of 100 characters allowed for classroom title");
    }
}
