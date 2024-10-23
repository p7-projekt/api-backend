using System.Text.Json.Serialization;

namespace Core.Exercises.Models
{
    public record Submission
    {
        [JsonPropertyName("solution")]
        public string Solution { get; }

        [JsonPropertyName("testCases")]
        public List<TestCase> TestCases { get; }

        public Submission(ExerciseDto exerciseDto)
        {
            Solution = exerciseDto.Solution;
            TestCases = new List<TestCase>();

            int i = 0;
            foreach (var testCase in exerciseDto.Testcases)
            {
                var inputParams = new List<Parameter>();
                for(int j = 0; j < testCase.inputParams.Length; j++)
                {
                    inputParams.Add(new Parameter(exerciseDto.InputParameterType[j], testCase.inputParams[j]));
                }

                var outputParams = new List<Parameter>();
                for (int j = 0; j < testCase.outputParams.Length; j++)
                {
                    outputParams.Add(new Parameter(exerciseDto.OutputParamaterType[j], testCase.outputParams[j]));
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
