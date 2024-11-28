using System.Security.Claims;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using FluentValidation;

namespace Core.Sessions.Validators;

public class JoinSessionDtoValidator : AbstractValidator<JoinDto>
{
    public JoinSessionDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage(x => $"{nameof(x.Code)} is required!");
        RuleFor(x => x.Code).Cascade(CascadeMode.Stop).Length(6).Must(ValidateCodeFormat).WithMessage(x => $"{nameof(x.Code)} is invalid");
    }

    private bool ValidateCodeFormat(string code)
    {
        return ValidateCodeFormatSession(code) || ValidateCodeFormatClassroom(code);
    }
    private bool ValidateCodeFormatSession(string code)
    {
        if (!(char.IsAsciiLetterUpper(code[0]) &&  char.IsAsciiLetterUpper(code[1]))) return false;
        if (!char.IsAsciiDigit(code[2])) return false;
        if (!char.IsAsciiDigit(code[3])) return false;
        if (!char.IsAsciiDigit(code[4])) return false;
        if (!char.IsAsciiDigit(code[5])) return false;
        
        return true;
    }
    private bool ValidateCodeFormatClassroom(string code)
    {
        if (!char.IsAsciiDigit(code[0])) return false;
        if (!char.IsAsciiDigit(code[1])) return false;
        if (!char.IsAsciiDigit(code[2])) return false;
        if (!char.IsAsciiDigit(code[3])) return false;
        if (!(char.IsAsciiLetterUpper(code[4]) && char.IsAsciiLetterUpper(code[5]))) return false;

        return true;
    }
}