using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface IUserService
{
	Task CreateUserAsync(string email, string password);
	Task<Result<LoginResponse>> LoginAsync(LoginDto loginDto);
}