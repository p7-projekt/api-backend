namespace Infrastructure.Authentication.Models;

public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt);