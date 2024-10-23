namespace Core.Exercises.Models
{
    public record ExerciseDto(
        string Name,
        string Description,
        string Solution,
        string[] InputParameterType,
        string[] OutputParamaterType,
        List<Testcase> Testcases);

    public record Testcase(string[] inputParams, string[] outputParams);
}
