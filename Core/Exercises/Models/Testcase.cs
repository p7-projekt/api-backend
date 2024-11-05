namespace Core.Exercises.Models;

public class Testcase
{
	public int TestCaseId { get; set; }

	public int ExerciseId { get; set; }

	public int TestCaseNumber { get; set; }
	public bool IsPublicVisible { get; set; }
	public List<TestParameter> Input { get; set; } = new();

	public List<TestParameter> Output { get; set; } = new();
}

public class TestParameter
{
	public int ParameterId { get; set; }
	public int TestCaseId { get; set; }
	public int ArgumentNumber { get; set; }
	public string ParameterType { get; set; } = string.Empty;
	public string ParameterValue { get; set; } = string.Empty;
	public bool IsOutput { get; set; }
}

public static class TestcaseMapper
{
	public static TestcaseDto ToTestcaseDto(this Testcase tcEntity)
	{
		return new TestcaseDto(tcEntity.Input.Select(x => x.ParameterValue).ToArray(), tcEntity.Output.Select(x => x.ParameterValue).ToArray(), tcEntity.IsPublicVisible);
	}
}
