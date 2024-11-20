using Core.Solutions.Models;
using Core.Solutions.Validators;

namespace UnitTest.Core.Solutions;

public class SubmitSolutionDtoValidatorTest
{
    [Fact]
    public void SubmitSolutionDtoValidatorTest_ShouldReturn_Ok()
    {
        var validator = new SubmitSolutionDtoValidator();
        var dto = new SubmitSolutionDto(1, "test", 1);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void SubmitSolutionDtoValidatorTest_ShouldReturn_FailEmptySolution()
    {
        var validator = new SubmitSolutionDtoValidator();
        var dto = new SubmitSolutionDto(1, "", 1);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void SubmitSolutionDtoValidatorTest_ShouldReturn_FailId()
    {
        var validator = new SubmitSolutionDtoValidator();
        var dto = new SubmitSolutionDto(0, "test", 1);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}