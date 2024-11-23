using Core.Classrooms.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Validators;

public class JoinClassroomDtoValidator : AbstractValidator<JoinClassroomDto>
{
    public JoinClassroomDtoValidator()
    {
        RuleFor(x => x.RoomCode).NotEmpty().WithMessage("Roomcode must be provided");
        RuleFor(x => x.RoomCode).Length(6).WithMessage("Roomcode must be correct length");
        RuleFor(x => x.RoomCode).Must(ValidateCodeFormat).WithMessage("Roomcode must be correct format");
    }

    private bool ValidateCodeFormat(string code)
    {
        if (!char.IsAsciiDigit(code[0])) return false;
        if (!char.IsAsciiDigit(code[1])) return false;
        if (!char.IsAsciiDigit(code[2])) return false;
        if (!char.IsAsciiDigit(code[3])) return false;
        if (!(char.IsAsciiLetterUpper(code[4]) && char.IsAsciiLetterUpper(code[5]))) return false;

        return true;
    }
}
