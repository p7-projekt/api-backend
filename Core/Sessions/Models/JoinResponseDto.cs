using System.Text.Json.Serialization;

namespace Core.Sessions.Models;

public record JoinResponseDto(
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Token, 
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	DateTime? ExpiresAt,
	JoinedType JoinedType,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	int? JoinedId
    );

// NOT SURE WHERE TO PLACE THIS ENUM
public enum JoinedType
{
	TimedSession = 1,
	Classroom = 2
}
