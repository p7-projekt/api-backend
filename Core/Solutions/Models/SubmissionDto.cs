﻿using System.Text.Json.Serialization;
using Core.Exercises.Models;
using Core.Languages.Models;

namespace Core.Solutions.Models;
public record SubmissionDto
{
    [JsonPropertyName("solution")]
    public string Solution { get; }

    [JsonPropertyName("languageId")]
    public Language Language { get; set; }

    [JsonPropertyName("testCases")]
    public List<SubmissionTestCase> TestCases { get; }

    public SubmissionDto(string solution, List<SubmissionTestCase> testCases)
    {
        Solution = solution;
        TestCases = testCases;
    }
    public SubmissionDto(ExerciseDto dto)
    {
        Solution = dto.Solution;
        TestCases = new List<SubmissionTestCase>();

        int i = 0;
        foreach (var testCase in dto.Testcases)
        {
            var inputParams = new List<Parameter>();
            for (int j = 0; j < testCase.InputParams.Length; j++)
            {
                inputParams.Add(new Parameter(dto.InputParameterType[j], testCase.InputParams[j]));
            }

            var outputParams = new List<Parameter>();
            for (int j = 0; j < testCase.OutputParams.Length; j++)
            {
                outputParams.Add(new Parameter(dto.OutputParameterType[j], testCase.OutputParams[j]));
            }

            TestCases.Add(new SubmissionTestCase(i, inputParams, outputParams));
            i++;
        }
    }
}

public static class SubmissionMapper {
    public static SubmissionDto ToSubmission(List<Testcase> testCases, string solution)
    {
        var testDtos = testCases.Select(x => new SubmissionTestCase(x.TestCaseId, 
            x.Input.Select(y => new Parameter(y.ParameterType, y.ParameterValue)).ToList(),
            x.Output.Select(y => new Parameter(y.ParameterType, y.ParameterValue)).ToList()
            )).ToList();
        return new SubmissionDto(solution, testDtos);
    }
}

public record SubmissionTestCase
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("inputParameters")]
    public List<Parameter> InputParameters { get; }

    [JsonPropertyName("outputParameters")]
    public List<Parameter> OutputParameters { get; }

    public SubmissionTestCase(int id, List<Parameter> inputParameters, List<Parameter> outputParameters)
    {
        Id = id;
        InputParameters = inputParameters;
        OutputParameters = outputParameters;
    }
}

public record Parameter
{
    [JsonPropertyName("valueType")]
    public string ValueType { get; }

    [JsonPropertyName("value")]
    public string Value { get; }

    internal Parameter(string valueType, string value)
    {
        ValueType = valueType;
        Value = value;
    }
}
