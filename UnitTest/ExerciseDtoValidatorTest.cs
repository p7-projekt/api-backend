using Core.DTOs;
using Core.DTOvalidators;
using System.ComponentModel;

namespace ExerciseDtoValidatorTest;

public class ExerciseDtoValidatorTest
{
    [Theory]
    [InlineData("Add numbers", "Concise exercise description", "x + y", new string[] { "int", "int" }, new string[] { "int" }, new string[] { "2", "2" }, new string[] { "4" }, new string[] { "10", "10" }, new string[] { "20" })]
    public void ExerciseDtoValidatorTest_ShouldBe_Valid(string title, string description, string solution, string[] inputParams, string[] outputParams, string[] testcase1InParam, string[] testcase1OutParam,  string[] testcase2InParam, string[] testcase2OutParam)
    {
        var validator = new ExerciseDtoValidator();
        var testcases = new List<(string[], string[])> { (testcase1InParam, testcase1OutParam), (testcase2InParam, testcase2OutParam)};
        var dto = new ExerciseDto(title, description, solution, inputParams, outputParams, testcases);


    }
}