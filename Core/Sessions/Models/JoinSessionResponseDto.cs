namespace Core.Sessions.Models;

public record JoinSessionResponseDto(string Token, DateTime ExpiresAt);