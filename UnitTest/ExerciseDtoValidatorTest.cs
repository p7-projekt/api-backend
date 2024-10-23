using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Core;
using System.ComponentModel;
using Core.Exercises.Models;
using Core.Exercises.Validators;

namespace ExerciseDtoValidatorTest;

public class ExerciseDtoValidatorTest
{
    private readonly ILogger<ExerciseDtoValidator> _loggerSubstitute = Substitute.For<ILogger<ExerciseDtoValidator>>();
    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "INT", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, new string[] { "10", "10" }, new string[] { "20" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "string", "string" }, new string[] { "int" }, new string[] { "This is a string", "This is another string" }, new string[] { "4" }, new string[] { "Second case 1", "Second case 2" }, new string[] { "20" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "float", "int" }, new string[] { "string" }, new string[] { "3.14", "14" }, new string[] { "correct" }, new string[] { "5.01", "0" }, new string[] { "Also Correct" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "bool", "char" }, new string[] { "char" }, new string[] { "false", "b" }, new string[] { "h" }, new string[] { "true", "u" }, new string[] { "l" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "string", "float" }, new string[] { "bOOl" }, new string[] { "This is a string", "4.20" }, new string[] { "true" }, new string[] { "Second testcase", "100100.5" }, new string[] { "false" })]
    public void ExerciseDtoValidatorTest_ShouldBe_Valid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam,  string[] testcase2InParam, string[] testcase2OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcase2 = new Testcase(testcase2InParam, testcase2OutParam);
        var testcases = new List<Testcase> { testcase1, testcase2 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData(null, "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_MissingTitel_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam) 
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1};
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Titel with more than 100 charactersTitel with more than 100 charactersTitel with more than 100 characters", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_TitelTooLong_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add numbers", null,"x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_MissingDescription_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", null, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_MissinggInputParameterType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, null, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_MissinggOutputParameterType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "Integer", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "Double", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "Array" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_InputParamterInvalidType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Integer" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Nonsense" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "Double" }, new string[] { "2", "2" }, new string[] { "4" })]
    public void ExerciseDtoValidatorTest_OutputParamterInvalidType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add Numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" })]
    public void ExerciseDtoValidatorTest_MissingTestcases_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcases = new List<Testcase> { };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, new string[] { "10", "10" }, new string[] { })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { "4" }, new string[] { "10", "10" }, new string[] { "20" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { "4" }, new string[] { "10", "10" }, new string[] { null })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { }, new string[] { null }, new string[] { "10", "10" }, new string[] { "20" })]
    public void ExerciseDtoValidatorTest_MissingTestcaseParameter_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, string[] testcase2InParam, string[] testcase2OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcase2 = new Testcase(testcase2InParam, testcase2OutParam);
        var testcases = new List<Testcase> { testcase1, testcase2 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, new string[] { "10", "10", "20" }, new string[] { "20" }, new string[] {"1", "2", "3"}, new string[] {"5", "false"})]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, new string[] { "20" }, new string[] { "20", "true" }, new string[] {"1", "2", "3"}, new string[] {"5", "false"})]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "false" }, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, new string[] {"1", "2"}, new string[] {"5"})]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2"}, new string[] { "4", "false" }, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, new string[] {"1", "2", "3"}, new string[] {"5", "false"})]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int", "int" }, new string[] { "int", "bool" }, new string[] { "2", "2", "5" }, new string[] { "4", "false" }, new string[] { "10", "10", "20" }, new string[] { "20", "true" }, new string[] {"1", "2", "3", "4"}, new string[] {"5", "false"})]
    public void ExerciseDtoValidatorTest_InconsistentParameterAmount_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam, string[] testcase2InParam, string[] testcase2OutParam, string[] testcase3InParam, string[] testcase3OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcase2 = new Testcase(testcase2InParam, testcase2OutParam);
        var testcase3 = new Testcase(testcase3InParam, testcase3OutParam);
        var testcases = new List<Testcase> { testcase1, testcase2, testcase3 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2.0", "2" }, new string[] { "4" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "bool" }, new string[] { "2", "2" }, new string[] { "4" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "char" }, new string[] { "int" }, new string[] { "2", "notchar" }, new string[] { "4" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "float", "int" }, new string[] { "int" }, new string[] { "notfloat", "2" }, new string[] { "4" })]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "not int" }, new string[] { "4" })]


    public void ExerciseDtoValidatorTest_ParameterValueDoesNotMatchDeclaredType_ShouldBe_Invalid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam)
    {
        var validator = new ExerciseDtoValidator(_loggerSubstitute);
        var testcase1 = new Testcase(testcase1InParam, testcase1OutParam);
        var testcases = new List<Testcase> { testcase1 };
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}

