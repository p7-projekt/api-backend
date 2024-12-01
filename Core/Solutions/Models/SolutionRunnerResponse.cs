using System.Text.Json.Serialization;

namespace Core.Solutions.Models;


public record SolutionResponseDto(
	string Result);
public class SolutionRunnerResponse
{
	public ResponseCode Action { get; set; }

	public string ResponseBody { get; set; } = string.Empty;
}

public enum ResponseCode
{
	Pass,
	Failure,
	Error,
}
