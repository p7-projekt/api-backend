using Core.Classrooms.Models;
using Core.Classrooms.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Core.Classrooms;

public class UpdateClassroomDtoValidatorTest
{
    [Theory]
    [InlineData("Valid Title", "Valid Description", true)]
    [InlineData("lowercase title", "Valid Description", true)]
    [InlineData("Valid Title", null, false)]
    [InlineData("Valid Title", "", false)]
    [InlineData("CAPS AND NUMBERS AND SPECIALS !?!", "Valid Description", true)]
    public void UpdateClassroomDtoValidatorTest_ShouldBe_Valid(string title, string description, bool registrationOpen)
    {
        var validator = new UpdateClassroomDtoValidator();
        var dto = new UpdateClassroomDto(title, description, registrationOpen);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateClassroomDtoValidatorTest_MissingTitle_ShouldBe_Invalid(string title)
    {
        var validator = new UpdateClassroomDtoValidator();
        var dto = new UpdateClassroomDto(title, "Description", true);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomDtoValidatorTest_TitleTooLong_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomDtoValidator();
        var tooLongTitle = "ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345!";
        var dto = new UpdateClassroomDto(tooLongTitle, "Description", true);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    // RegistrationOpen field is not nullable, why a testcse for failing that rule cannot be conducted.
}
