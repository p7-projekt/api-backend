using System.Text.Json.Serialization;

namespace Core.Solutions.Models;


public record MozartHaskellResponseDto(
	string Result,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Message,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	List<MozartHaskellTestcaseResultDto>? TestCaseResults);

public record MozartHaskellTestcaseResultDto(
	int Id,
	string TestResult,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Cause,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	MozartHaskellTestcaseDetailsDto? Details);

public record MozartHaskellTestcaseDetailsDto(
	List<MozartHaskellTestcaseInputParameters> InputParameters,
	string Actual,
	string Expected);

public record MozartHaskellTestcaseInputParameters(string ValueType, string Value);

public record HaskellResponseDto(
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	List<MozartHaskellTestcaseResultDto>? TestCaseResults,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Message);

public class HaskellResponse
{
	public ResponseCode Action { get; set; }
	public MozartHaskellResponseDto? ResponseDto { get; set; }
}

public enum ResponseCode
{
	Pass,
	Failure,
	Error,
}
