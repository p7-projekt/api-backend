using Core.Classrooms.Models;
using Core.Classrooms.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Core.Classrooms;

public class ClassroomDtoValidatorTest
{
    [Theory]
    [InlineData("Valid Title", "Valid Description")]
    [InlineData("lowercase title", "Valid Description")]
    [InlineData("Valid Title", null)]
    [InlineData("Valid Title", "")]
    [InlineData("CAPS AND NUMBERS AND SPECIALS !?!", "Valid Description")]
    public void ClassroomDtoValidatorTest_ShouldBe_Valid(string title, string description)
    {
        var validator = new ClassroomDtoValidator();
        var dto = new ClassroomDto(title, description);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ClassroomDtoValidatorTest_MissingTitle_ShouldBe_Invalid(string title)
    {
        var validator = new ClassroomDtoValidator();
        var dto = new ClassroomDto(title, "description");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomDtoValidatorTest_TitleTooLong_ShouldBe_Invalid()
    {
        var validator = new ClassroomDtoValidator();
        var tooLongTitle = "ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345!";
        var dto = new ClassroomDto(tooLongTitle, "description");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);

    }
}
