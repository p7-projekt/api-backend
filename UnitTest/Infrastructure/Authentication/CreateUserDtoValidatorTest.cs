using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Infrastructure.Authentication.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Infrastructure.Authentication;

public class CreateUserDtoValidatorTest
{
    [Theory]
    [InlineData("validmail@mail.com", "Password1!", "Password1!", "James")]
    [InlineData("anotheremail@yahoo.dk", "!?4hEJ", "!?4hEJ", "k1ng")]
    [InlineData("ninja@hotmail.com", "Cola123!", "Cola123!", "123")]
    public void CreateUserDtoValidator_ShouldBe_Valid(string email, string password, string confirmPassword, string name)
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var dto = new CreateUserDto(email, password, confirmPassword, name);

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.True(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_InvalidEmail_ShouldBe_InValid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var invalidEmail = "invalidmailmail.com"; 
        var dto = new CreateUserDto(invalidEmail, "Password1!", "Password1!", "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_EmailInUse_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(false);

        var dto = new CreateUserDto("validmail@mail.com", "Password1!", "Password1!", "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Theory]
    [InlineData("", "Password1!")]
    [InlineData("Password1!", "")]
    [InlineData("    ", "Password1!")]
    [InlineData("Password1!", "    ")]
    public void CreateUserDtoValidator_MissingPassword_ShouldBe_Invalid(string password, string confirmPassword)
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var dto = new CreateUserDto("validmail@mail.com", password, confirmPassword, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordTooShort_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password = "Aa1!";
        var dto = new CreateUserDto("validmail@mail.com", password, password, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordMissingUpperCase_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password = "aaa123!";
        var dto = new CreateUserDto("validmail@mail.com", password, password, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordMissingLowerCase_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password = "AAA123!";
        var dto = new CreateUserDto("validmail@mail.com", password, password, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordMissingDigit_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password = "AaaBbb!";
        var dto = new CreateUserDto("validmail@mail.com", password, password, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordMissingSpecialCharacter_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password = "Aaa123";
        var dto = new CreateUserDto("validmail@mail.com", password, password, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_PasswordsNotMatching_ShouldBe_Invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var password1 = "Aaa123!";
        var password2 = "Bbb345?";
        var dto = new CreateUserDto("validmail@mail.com", password1, password2, "James");

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_MissingName_Shouldbe_invalid()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var validator = new CreateUserDtoValidator(userRepo);

        userRepo.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);

        var name = string.Empty;
        var dto = new CreateUserDto("validmail@mail.com", "Aaa123!", "Aaa123!", name);

        var result = validator.ValidateAsync(dto);

        Assert.True(result.IsCompletedSuccessfully);
        Assert.False(result.Result.IsValid);
    }
}
