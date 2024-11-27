using System.Text.Json.Serialization;

namespace Core.Sessions.Models;

public record JoinSessionResponseDto(
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Token, 
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	DateTime? ExpiresAt);