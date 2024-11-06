using Dapper;
using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class TokenRepository : ITokenRepository
{
	private readonly IDbConnectionFactory _connection;
	private readonly ILogger<TokenRepository> _logger;

	public TokenRepository(IDbConnectionFactory connection, ILogger<TokenRepository> logger)
	{
		_connection = connection;
		_logger = logger;
	}

	public async Task InsertTokenAsync(RefreshToken token)
	{
		var query = """
		            INSERT INTO refresh_token (token, expires, created_at, user_id) VALUES (@token, @expires, @created_at, @userId);
		            """;
		using var con = await _connection.CreateConnectionAsync();
		_logger.LogInformation("Token added for userid: {user}, expires {expiration}", token.UserId, token.Expires);
		await con.ExecuteAsync(query, new { token = token.Token, expires = token.Expires, created_at = token.CreatedAt, userId = token.UserId });
	}

	public async Task DeleteRefreshTokenByUserIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            DELETE FROM refresh_token WHERE user_id = @UserId;
		            """;
		await con.ExecuteAsync(query, new { userId });
	}
	
	public async Task<Result<RefreshToken>> GetAccessTokenByRefreshTokenAsync(string refreshToken)
	{
		// check token exists based on userid
		var query = """
		            SELECT id, token, expires, created_at AS createdat, user_id AS userid FROM refresh_token WHERE token = @refreshToken
		            """;
		using var con = await _connection.CreateConnectionAsync();
		var token = await con.QuerySingleOrDefaultAsync<RefreshToken>(query, new {refreshToken = refreshToken});
		if (token == null)
		{
			return Result.Fail("Failed to retrieve a token");
		}
		_logger.LogInformation("Refresh token: {token} exists for userid: {userid}", refreshToken, token.UserId);
		// check expiration 
		if (!await CheckValidRefreshToken(token.Token))
		{
			return Result.Fail("Token was not valid");
		}
		return Result.Ok(token);
	}
	
	private async Task<bool> CheckValidRefreshToken(string refreshToken)
	{
		var query = """
		            DELETE FROM refresh_token WHERE expires <= NOW() AND token = @refreshToken
		            """;
		using var con = await _connection.CreateConnectionAsync();
		var rowsAffected = await con.ExecuteAsync(query, new {refreshToken});
		return rowsAffected == 0;
	}
}