using System.Text.Json.Serialization;
using Core.Exercises.Models;

namespace Core.Solutions.Models
{
    public record Submission
    {
        [JsonPropertyName("solution")]
        public string Solution { get; }

        [JsonPropertyName("testCases")]
        public List<TestCase> TestCases { get; }

        public Submission(ExerciseSubmissionDto dto)
        {
            Solution = dto.Solution;
            TestCases = new List<TestCase>();

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
                    outputParams.Add(new Parameter(dto.OutputParamaterType[j], testCase.OutputParams[j]));
                }

                TestCases.Add(new TestCase(i, inputParams, outputParams));
                i++;
            }
        }
    }

    public record TestCase
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("inputParameters")]
        public List<Parameter> InputParameters { get; }

        [JsonPropertyName("outputParameters")]
        public List<Parameter> OutputParameters { get; }

        internal TestCase(int id, List<Parameter> inputParameters, List<Parameter> outputParameters)
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
}
