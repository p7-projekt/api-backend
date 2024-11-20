using System.Data;
using Core.Shared;
using Dapper;
using FluentResults;
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
		            SELECT * FROM users
		            WHERE id = @Id;
		            """;
		return await con.QueryFirstOrDefaultAsync<User>(query, new { Id = userId });
	}

	/*public async Task<User?> GetAppUserByIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            SELECT email, name
		            FROM app_users
		            WHERE user_id = @id;
		            """;
		var user = await con.QuerySingleAsync<User>(query, new { id = userId });
		return user;
	}*/

	public async Task<int> GetAnonUserSessionByIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            SELECT session_id FROM user_in_timedsession WHERE user_id = @UserId;
		            """;
		var sessionId = await con.QuerySingleOrDefaultAsync<int>(query, new { userId });
		return sessionId;
	}

	public async Task<User?> GetUserByEmailAsync(string email)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
					SELECT id, email, password_hash AS passwordhash, created_at AS createdat
					FROM users
					WHERE email = @Email;
					""";
		var user = await con.QuerySingleOrDefaultAsync<User>(query, new { email });
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
					FROM users
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
			//Create base user and role
			var userId = await CreateUserAndRoleAsync(user, role, con, transaction);

			_logger.LogInformation("Committing transaction");
			transaction.Commit();
		}
		catch (Exception e)
		{
			_logger.LogInformation("Creating user with email: {email} failed, Ex msg: {errormsg}", user.Email, e.Message);
			transaction.Rollback();
			return Result.Fail("Failed to create user");
		}

		return Result.Ok();
	}
	
	public async Task<Result<int>> CreateAnonUserAsync(string name, int sessionId)
	{
		using var con = await _connection.CreateConnectionAsync();
		using var transaction = con.BeginTransaction();
		var userId = 0;
		try
		{
			//Create base user and role
			userId = await CreateUserAndRoleAsync(new User {Name = name, CreatedAt = DateTime.UtcNow}, Roles.AnonymousUser, con, transaction);
			
			// insert into user_in_session
			var query = """
			            INSERT INTO user_in_timedsession (user_id, session_id)
			            VALUES 
			            (@UserId, @SessionId);
			            """;
			await con.ExecuteAsync(query, new { UserId = userId, SessionId = sessionId}, transaction);
			
			_logger.LogInformation("Committing transaction");
			transaction.Commit();
		}
		catch (Exception)
		{
			_logger.LogInformation("Creating anonymous user for session: {sessionId} failed", sessionId);
			transaction.Rollback();
			return Result.Fail("Failed to create user");
		}

		return Result.Ok(userId);
	}

	private async Task<int> CreateUserAndRoleAsync(User user, Roles role, IDbConnection con, IDbTransaction transaction)
	{
		var userId = 0;
		if (role == Roles.AnonymousUser)
		{
			userId = await InsertAnonUserAsync(con, transaction, user);
		}
		else
		{
			userId = await InsertUserAsync(con, transaction, user);
		}
		
		
		var createRoleQuery = """
		                      INSERT INTO user_role (user_id, role_id)
		                      SELECT @userId, id FROM role WHERE name = @roleName;
		                      """;
		_logger.LogInformation("Inserting role in user_roles: {query}", createRoleQuery);
		await con.ExecuteAsync(createRoleQuery, new {userId = userId, roleName = role.ToString()}, transaction);
		return userId;
	}

	private async Task<int> InsertUserAsync(IDbConnection con, IDbTransaction transaction, User user)
	{
		var createAppUser = """
		                    INSERT INTO users 
		                        (email, name, password_hash, created_at, anonymous) 
		                    VALUES 
		                        (@email, @Name, @password_hash, @CreatedAt, @Anon)
		                    RETURNING id;
		                    """;
		_logger.LogInformation("Inserting app user into transaction: {query}", createAppUser);
		return await con.QueryFirstOrDefaultAsync<int>(createAppUser, new {email = user.Email, Name = user.Name, password_hash = user.PasswordHash, Anon = false, CreatedAt = user.CreatedAt}, transaction);
	}

	private async Task<int> InsertAnonUserAsync(IDbConnection con, IDbTransaction transaction, User user)
	{
		var createAnonUserQuery= """
		                        INSERT INTO users 
		                        (name, anonymous, created_at) 
		                        VALUES (@Name, @Anon, @CreatedAt)
		                        RETURNING id;
		                        """;
		_logger.LogInformation("Inserting anon user into transaction: {query}", createAnonUserQuery);
		return await con.QueryFirstOrDefaultAsync<int>(createAnonUserQuery, new {Name = user.Name, Anon = true, CreatedAt = user.CreatedAt}, transaction);
	}
}
