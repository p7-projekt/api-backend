using System.Data;
using Dapper;
using FluentResults;
using FluentValidation.Validators;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class UserRepository : IUserRepository
{
	private readonly IDbConnectionFactory _connection;
	private readonly ILogger<UserRepository> _logger;
	
	public UserRepository(IDbConnectionFactory connection, ILogger<UserRepository> logger)
	{
		_connection = connection;
		_logger = logger;
	}

	public async Task<User?> GetUserByIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            SELECT * FROM users WHERE id = @id;
		            """;
		var user = await con.QueryAsync<User>(query, new { id = userId });
		return user.FirstOrDefault();
	}

	public async Task<User?> GetUserByEmailAsync(string email)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
					SELECT u.id, au.email, au.password_hash AS passwordhash, u.created_at AS createdat
					FROM users AS u
					         JOIN app_users AS au
					              ON u.id = au.user_id
					WHERE email = @Email;
					""";
		var user = await con.QuerySingleAsync<User>(query, new { email });
		return user;
	}
	
	public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            SELECT role.name as rolename FROM user_role
		            JOIN role ON user_role.role_id = role.id AND user_role.user_id = @userId
		            """;
		_logger.LogInformation("Selecting roles for userid: {userid}", userId);
		var roles = await con.QueryAsync<Role>(query, new { userId = userId });
		return roles;
	}

	public async Task<bool> IsEmailAvailableAsync(string email)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
					SELECT COUNT(email) 
					FROM users AS u
					JOIN app_users AS au
						ON u.id = au.user_id
					WHERE email = @email;
					""";
		_logger.LogInformation("Checking if email {email} is available", email);
		var result = await con.QueryAsync<int>(query, new { email = email });
		return result.First() == 0;
	}

	public async Task<Result> CreateUserAsync(User user, Roles role)
	{
		using var con = await _connection.CreateConnectionAsync();
		using var transaction = con.BeginTransaction();
		try
		{
			var userInsertQuery = """
			                      INSERT INTO users (created_at) VALUES (@CreatedAt) 
			                      RETURNING id;
			                      """;
			_logger.LogInformation("Inserting user into transaction: {query}", userInsertQuery);
			var userId = await con.ExecuteScalarAsync<int>(userInsertQuery, new {user.CreatedAt}, transaction);
			var createAppUser = """
			                 INSERT INTO app_users 
			                     (user_id, email, password_hash) 
			                 VALUES 
			                     (@UserId, @email, @password_hash);
			                 """;
			_logger.LogInformation("Inserting app user into transaction: {query}", createAppUser);
			await con.ExecuteAsync(createAppUser, new {userId = userId, password_hash = user.PasswordHash}, transaction);
			
			var createRoleQuery = """
			                      INSERT INTO user_role (user_id, role_id)
			                      SELECT @userId, id FROM role WHERE name = @roleName;
			                      """;
			_logger.LogInformation("Inserting role in user_roles: {query}", createRoleQuery);
			await con.ExecuteAsync(createRoleQuery, new {userId = userId, roleName = role.ToString()});
			
			_logger.LogInformation("Committing transaction");
			transaction.Commit();
		}
		catch (Exception)
		{
			_logger.LogInformation("Creating user with email: {email} failed", user.Email);
			transaction.Rollback();
			return Result.Fail("Failed to create user");
		}

		return Result.Ok();
	}
}
