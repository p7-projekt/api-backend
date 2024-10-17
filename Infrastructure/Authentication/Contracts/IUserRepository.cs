using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface IUserRepository
{
	Task<User?> GetUserByIdAsync(int userId);
	Task<User?> GetUserByEmailAsync(string email);
	Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
	Task<bool> IsEmailAvailableAsync(string email);
	Task<Result> CreateUserAsync(User user, Roles role);
}