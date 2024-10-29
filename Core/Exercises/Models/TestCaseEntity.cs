namespace Core.Exercises.Models;

public class TestCaseEntity
{
	public int TestCaseId { get; set; }

	public int ExerciseId { get; set; }

	public int TestCaseNumber { get; set; }

	public List<TestParameterEntity> Input { get; set; } = new();

	public List<TestParameterEntity> Output { get; set; } = new();
}

public class TestParameterEntity
{
	public int ParameterId { get; set; }
	public int TestCaseId { get; set; }
	public int ArgumentNumber { get; set; }
	public string ParameterType { get; set; } = string.Empty;
	public string ParameterValue { get; set; } = string.Empty;
	public bool IsOutput { get; set; }
}
