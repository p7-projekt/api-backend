using Core.Classrooms.Models;
using Core.Classrooms.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Core.Classrooms;

public class UpdateClassroomSessionDtoValidatorTest
{
    [Theory]
    [InlineData(1, "Valid Title", "Valid Description", true)]
    [InlineData(2, "lowercase title", "Valid Description", true)]
    [InlineData(10, "Valid Title", null, false)]
    [InlineData(17, "Valid Title", "", false)]
    [InlineData(111, "CAPS AND NUMBERS AND SPECIALS !?!", "Valid Description", true)]
    public void UpdateClassroomSessionDtoTest_ShouldBe_Valid(int id, string title, string description, bool active)
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var dto = new UpdateClassroomSessionDto(id, title, description, active, new List<int> { 1, 2, 3}, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_InvalidId_Shouldbe_Invalid()
    {
        {
            var validator = new UpdateClassroomSessionDtoValidator();
            var dto = new UpdateClassroomSessionDto(0, "ValidTitle", "Description", true, new List<int> { 1, 2, 3 }, new List<int> { 1, 2 });

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateClassroomSessionDtoTest_MissingTitle_ShouldBe_Invalid(string title)
    {
        {
            var validator = new UpdateClassroomSessionDtoValidator();
            var dto = new UpdateClassroomSessionDto(1, title, "Description", true, new List<int> { 1, 2, 3 }, new List<int> { 1, 2 });

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_TitleTooLong_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var tooLongTitle = "ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345ABCDE12345!";
        var dto = new UpdateClassroomSessionDto(1, tooLongTitle, "Description", true, new List<int> { 1, 2, 3 }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_MissingExercises_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var dto = new UpdateClassroomSessionDto(1, "Valid Title", "Description", true, new List<int> { }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_NegativeExerciseId_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var dto = new UpdateClassroomSessionDto(1, "Valid Title", "Description", true, new List<int> { 1, 2, -3 }, new List<int> { 1, 2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_Missinglanguage_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var dto = new UpdateClassroomSessionDto(1, "Valid Title", "Description", true, new List<int> { 1, 2, 3 }, new List<int> { });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateClassroomSessionDtoTest_NegativeLanguageId_ShouldBe_Invalid()
    {
        var validator = new UpdateClassroomSessionDtoValidator();
        var dto = new UpdateClassroomSessionDto(1, "Valid Title", "Description", true, new List<int> { 1, 2, 3 }, new List<int> { 1, -2 });

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
