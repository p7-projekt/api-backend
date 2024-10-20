using FluentValidation;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
	private readonly IUserRepository _userRepository;
	public CreateUserDtoValidator(IUserRepository userRepository)
	{
		_userRepository = userRepository;
		RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress().WithMessage("Email is required")
			.MustAsync(IsEmailAvailableAsync).WithMessage("Email already used");
		RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
		RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters");
		RuleFor(x => x.Password).Cascade(CascadeMode.Continue).Must(CheckUpperCase).WithMessage("Password must contain at least one upper case letter").Must(CheckLowerCase).WithMessage("Password must contain at least one lower case letter").Must(CheckDigit).WithMessage("Password must contain at least one digit").Must(CheckSpecialCharacter).WithMessage("Password must contain at least one special character");
		RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm password is required");
		RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password).WithMessage("Passwords do not match");
	}
	
	private async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken)
	{
		return await _userRepository.IsEmailAvailableAsync(email);
	}

	private bool CheckUpperCase(string password)
	{
		return password.Any(char.IsUpper);
	}

	private bool CheckLowerCase(string password)
	{
		return password.Any(char.IsLower);
	}

	private bool CheckDigit(string password)
	{
		return password.Any(char.IsDigit);
	}

	private bool CheckSpecialCharacter(string password)
	{
		var validChars = "!\"#¤%&/()=?+^'-.,><§½¡@£$½¥{[]}";
		return validChars.Any(password.Contains);
	}
}