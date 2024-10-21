using System.Security.Claims;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using FluentValidation;

namespace Core.Sessions.Validators;

public class JoinSessionDtoValidator : AbstractValidator<JoinSessionDto>
{
    public JoinSessionDtoValidator()
    {
        RuleFor(x => x.SessionCode).NotEmpty().WithMessage(x => $"{nameof(x.SessionCode)} is required!");
        RuleFor(x => x.SessionCode).Cascade(CascadeMode.Stop).Length(6).Must(ValidateCodeFormat).WithMessage(x => $"{nameof(x.SessionCode)} is invalid");
    }
    private bool ValidateCodeFormat(string code)
    {
        if (!(char.IsAsciiLetterUpper(code[0]) &&  char.IsAsciiLetterUpper(code[1]))) return false;
        if (!char.IsAsciiDigit(code[2])) return false;
        if (!char.IsAsciiDigit(code[3])) return false;
        if (!char.IsAsciiDigit(code[4])) return false;
        if (!char.IsAsciiDigit(code[5])) return false;
        
        return true;
    }
}