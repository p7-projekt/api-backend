using Core.Sessions.Models;

namespace Core.Exercises.Models;

public record ExerciseDto(
    string Name,
    string Description,
    string Solution,
    int Language,
    string[] InputParameterType,
    string[] OutputParamaterType,
    List<TestcaseDto> Testcases
);