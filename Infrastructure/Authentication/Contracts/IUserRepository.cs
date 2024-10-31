using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface IUserRepository
{
	Task<User?> GetAppUserByIdAsync(int userId);

	Task<int> GetAnonUserSessionByIdAsync(int userId);
	Task<User?> GetUserByEmailAsync(string email);
	Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
	Task<bool> IsEmailAvailableAsync(string email);
	Task<Result> CreateUserAsync(User user, Roles role);
	Task<Result<int>> CreateAnonUserAsync(int sessionId);
}