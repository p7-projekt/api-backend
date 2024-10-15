using FluentValidation;
using FluentValidation.Validators;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
	public LoginDtoValidator()
	{
		RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
		RuleFor(x => x.Email).EmailAddress().WithMessage("Email is invalid");
		RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
		RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must contain at least 6 characters");
	}
}