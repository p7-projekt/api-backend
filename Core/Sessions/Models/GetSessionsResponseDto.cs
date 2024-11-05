namespace Core.Sessions.Models;

public record GetSessionsResponseDto(int Id, string Title, string ExpiresInSeconds, string? SessionCode);