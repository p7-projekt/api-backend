using Core.Classrooms.Models;
using Core.Classrooms.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Core.Classrooms;

public class ClassroomSessionDtoValidatorTest
{
    [Theory]
    [InlineData("Valid Title", "Valid Description")]
    [InlineData("lowercase title", "Valid Description")]
    [InlineData("Valid Title", null)]
    [InlineData("Valid Title", "")]
    [InlineData("CAPS AND NUMBERS AND SPECIALS !?!", "Valid Description")]
    public void ClassroomSesssionDtoTest_ShouldBe_Valid(string title, string description)
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto(title, description, new List<int> {1, 2, 3}, new List<int> {1, 2});

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ClassroomSesssionDtoTest_MissingTitle_ShouldBe_Invalid(string title)
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto(title, "Description", new List<int> { 1, 2, 3 }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomSessionDtoTest_TitleTooLong_ShouldBe_Invalid()
    {
        var validator = new ClassroomSessionDtoValidator();
        var tooLongTitle = "ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345!";
        var dto = new ClassroomSessionDto(tooLongTitle, "Description", new List<int> { 1, 2, 3 }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomSessionDtoTest_MissingExercises_ShouldBe_Invalid()
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto("Valid title", "Description", new List<int> {}, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomSessionDtoTest_NegativeExerciseId_ShouldBe_Invalid()
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto("Valid title", "Description", new List<int> { 1, 2, -3 }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomSessionDtoTest_MissingLanguage_ShouldBe_Invalid()
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto("Valid title", "Description", new List<int> { 1, 2, 3 }, new List<int> { });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClassroomSessionDtoTest_NegativeLanguageId_ShouldBe_Invalid()
    {
        var validator = new ClassroomSessionDtoValidator();
        var dto = new ClassroomSessionDto("Valid title", "Description", new List<int> { 1, 2, 3 }, new List<int> { 1, -2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
