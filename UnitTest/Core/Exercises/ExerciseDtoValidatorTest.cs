using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Core;
using System.ComponentModel;
using Core.Exercises.Models;
using Core.Exercises.Validators;
using Xunit.Sdk;

namespace UnitTest.Core.Exercises;

public class ExerciseDtoValidatorTest
{
    private readonly ILogger<ExerciseDtoValidator> _loggerSubstitute = Substitute.For<ILogger<ExerciseDtoValidator>>();
    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "INT", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true, new string[] { "10", "10" }, new string[] { "20" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "string", "string" }, new string[] { "float" }, new string[] { "This is a string", "This is another string" }, new string[] { "4.13" }, true, new string[] { "Second case 1", "Second case 2" }, new string[] { "20" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "float", "int" }, new string[] { "string" }, new string[] { "3.14", "14" }, new string[] { "correct" }, true, new string[] { "5.01", "0" }, new string[] { "Also Correct" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "bool", "char" }, new string[] { "char" }, new string[] { "false", "b" }, new string[] { "h" }, false, new string[] { "true", "u" }, new string[] { "l" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "string", "float" }, new string[] { "bOOl" }, new string[] { "This is a string", "4.20" }, new string[] { "true" }, true, new string[] { "Second testcase", "100100.5" }, new string[] { "false" }, false)]
    public void ExerciseDtoValidatorTest_ShouldBe_Valid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible1, string[] testcase2InParam, string[] testcase2OutParam, bool publicVisible2)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible1);
        var testcase2 = new TestcaseDto(testcase2InParam, testcase2OutParam, publicVisible2);
        var testcases = new List<TestcaseDto> { testcase1, testcase2 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData(null, "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_MissingTitel_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisibile)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisibile);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Titel with more than 100 charactersTitel with more than 100 charactersTitel with more than 100 characters", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_TitelTooLong_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add numbers", null, "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_MissingDescription_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", null, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_MissinggInputParameterType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, null, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_MissinggOutputParameterType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "Integer", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "Double", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "Array" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_InputParamterInvalidType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Integer" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Nonsense" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Double" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_OutputParamterInvalidType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" })]
    public void ExerciseDtoValidatorTest_MissingTestcases_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcases = new List<TestcaseDto> { };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, true, new string[] { "10", "10" }, new string[] { }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { "4" }, true, new string[] { "10", "10" }, new string[] { "20" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { "4" }, true, new string[] { "10", "10" }, new string[] { null }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { null }, true, new string[] { "10", "10" }, new string[] { "20" }, true)]
    public void ExerciseDtoValidatorTest_MissingTestcaseParameter_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible1, string[] testcase2InParam, string[] testcase2OutParam, bool publicVisible2)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible1);
        var testcase2 = new TestcaseDto(testcase2InParam, testcase2OutParam, publicVisible2);
        var testcases = new List<TestcaseDto> { testcase1, testcase2 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, true, new string[] { "10", "10", "20" }, new string[] { "20" }, true, new string[] { "1", "2", "3" }, new string[] { "5", "false" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, true, new string[] { "20" }, new string[] { "20", "true" }, true, new string[] { "1", "2", "3" }, new string[] { "5", "false" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "false" }, true, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, true, new string[] { "1", "2" }, new string[] { "5" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2" }, new string[] { "4", "false" }, true, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, true, new string[] { "1", "2", "3" }, new string[] { "5", "false" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, true, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, true, new string[] { "1", "2", "3", "4" }, new string[] { "5", "false" }, true)]
    public void ExerciseDtoValidatorTest_InconsistentParameterAmount_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible1, string[] testcase2InParam, string[] testcase2OutParam, bool publicVisible2, string[] testcase3InParam, string[] testcase3OutParam, bool publicVisible3)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible1);
        var testcase2 = new TestcaseDto(testcase2InParam, testcase2OutParam, publicVisible2);
        var testcase3 = new TestcaseDto(testcase3InParam, testcase3OutParam, publicVisible3);
        var testcases = new List<TestcaseDto> { testcase1, testcase2, testcase3 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2.0", "2" }, new string[] { "4" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "bool" }, new string[] { "2", "2" }, new string[] { "4" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "char" }, new string[] { "int" }, new string[] { "2", "notchar" }, new string[] { "4" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "float", "int" }, new string[] { "int" }, new string[] { "notfloat", "2" }, new string[] { "4" }, true)]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "not int" }, new string[] { "4" }, true)]
    public void ExerciseDtoValidatorTest_ParameterValueDoesNotMatchDeclaredType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "INT", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, false)]
    public void ExerciseDtoValidatorTest_NoPublicVisibleTestcase(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, bool publicVisible)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new TestcaseDto(testcase1InParam, testcase1OutParam, publicVisible);
        var testcases = new List<TestcaseDto> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}

