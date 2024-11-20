using System.Text.Json.Serialization;

namespace Infrastructure.Authentication.Models;

public record GetUserResponseDto(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Email, 
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Name
    );