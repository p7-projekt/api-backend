using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface IUserService
{
	Task<Result> CreateUserAsync(CreateUserDto dto);
	Task<Result<LoginResponse>> LoginAsync(LoginDto loginDto);
	Task<Result<GetUserResponseDto>> GetAppUserByIdAsync(int id);
	Task<Result<GetUserResponseDto>> GetAnonUserByIdAsync(int userId);
}