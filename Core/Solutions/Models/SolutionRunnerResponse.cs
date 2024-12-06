using System.Text.Json.Serialization;

namespace Core.Solutions.Models;


public record SolutionResponseDto(
	string Result);
public class SolutionRunnerResponse
{
	public ResponseCode Action { get; set; }

	public dynamic? ResponseBody { get; set; }
}

public enum ResponseCode
{
	Pass,
	Failure,
	Error,
}
