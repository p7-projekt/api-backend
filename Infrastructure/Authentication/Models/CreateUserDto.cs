namespace Infrastructure.Authentication.Models;

public record CreateUserDto(string Email, string Password, string ConfirmPassword, string Name);