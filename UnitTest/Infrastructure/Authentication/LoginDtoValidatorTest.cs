using Infrastructure.Authentication.Models;
using Infrastructure.Authentication.Validators;

namespace UnitTest.Infrastructure.Authentication;

public class LoginDtoValidatorTest
{
	[Theory]
	[InlineData("Peter@login.dk", "UlluBullu")]
	[InlineData("P@l.dk", "123456")]
	[InlineData("a@.dk", "!4536sdasdf")]
	[InlineData("JensHansen@login.com", "SAFGHSJL123!!")]
	[InlineData("Mister@login.co.uk", "hjdfjhgfj")]
	public void LoginDtoValidatorTest_ShouldBe_Valid(string email, string password)
	{
		// Arrange
		var validator = new LoginDtoValidator();
		var record = new LoginDto(email, password);
		
		// Act
		var result = validator.Validate(record);

		// Assert
		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData("MyEmail.com", "1231412")]
	[InlineData("MyEmail@mail.com", "12314")]
	[InlineData("", "1231412")]
	[InlineData("MyEmail@mail.com", "")]
	[InlineData("@MyEmail", "ASFSDFSDFSDFsa")]
	[InlineData("M.dk", "1231412")]
	public void LoginDtoValidatorTest_ShouldNotBe_Valid(string email, string password)
	{
		// Arrange
		var validator = new LoginDtoValidator();
		var record = new LoginDto(email, password);
		
		// Act
		var result = validator.Validate(record);

		// Assert
		Assert.False(result.IsValid);
	}
}