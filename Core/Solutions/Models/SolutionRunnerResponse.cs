using System.Text.Json.Serialization;

namespace Core.Solutions.Models;


public record SolutionResponseDto(
	string Result,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Message,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	List<SolutionTestcaseResultDto>? TestCaseResults);

public record SolutionTestcaseResultDto(
	int Id,
	string TestResult,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Cause,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	SolutionTestcaseDetailsDto? Details);

public record SolutionTestcaseDetailsDto(
	List<SolutionTestcaseInputParameters> InputParameters,
	string Actual,
	string Expected);

public record SolutionTestcaseInputParameters(string ValueType, string Value);

public record MozartResponseDto(
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	List<SolutionTestcaseResultDto>? TestCaseResults,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Message);

public class SolutionRunnerResponse
{
	public ResponseCode Action { get; set; }
	public SolutionResponseDto? ResponseDto { get; set; }
}

public enum ResponseCode
{
	Pass,
	Failure,
	Error,
}
