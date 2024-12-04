using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Exceptions;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    public TokenService(ILogger<TokenService> logger, ITokenRepository tokenRepository, IUserRepository userRepository)
    {
        _logger = logger;
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
    }
    public string GenerateJwt(int userId, List<Roles> roles)
    {
        var env = Environment.GetEnvironmentVariable("PERFORMANCE_TEST");
        bool performanceTest = env != null && string.Equals("true", env, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("Performance test env: {performancetest}", performanceTest);
        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.UserData, userId.ToString()));
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }
        var token = new JwtSecurityToken(
            issuer: AuthConstants.Issuer,
            audience: AuthConstants.Audience,
            claims: claims,
            expires: performanceTest ? DateTime.UtcNow.AddHours(12) : DateTime.UtcNow.AddMinutes(AuthConstants.JwtExpirationInMinutes),
            signingCredentials: GetSigningCredentials()
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateAnonymousUserJwt(int sessionLength, int userId)
    {
        // todo: change this to load roles from anon users.
        var token = new JwtSecurityToken(
            issuer: AuthConstants.Issuer,
            audience: AuthConstants.Audience,
            claims: new List<Claim>
            {
                new Claim(ClaimTypes.UserData, userId.ToString()),
                new Claim(ClaimTypes.Role, nameof(Roles.AnonymousUser))
            },
            expires: DateTime.UtcNow.AddMinutes(sessionLength),
            signingCredentials: GetSigningCredentials()
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = Environment.GetEnvironmentVariable(AuthConstants.JwtSecret);
        if (key == null)
        {
            _logger.LogWarning("No jwt key environment variable found!");
            throw new MissingJwtKeyException("No jwt key environment variable found!");
        }
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    public async Task<Result<RefreshToken>> GenerateRefreshToken(int userId)
    {
        // if token exist remove it.
        await _tokenRepository.DeleteRefreshTokenByUserIdAsync(userId);
        
        // Create new
        var refreshToken = new RefreshToken
        {
            Token = CreateRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpirationInDays),
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
        await _tokenRepository.InsertTokenAsync(refreshToken);
        return refreshToken;
    }

    private string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return string.Concat(Convert.ToBase64String(randomNumber), Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
    }

    public async Task<Result<LoginResponse>> GenerateJwtFromRefreshToken(RefreshDto refreshToken)
    {
        var token = await _tokenRepository.GetAccessTokenByRefreshTokenAsync(refreshToken.RefreshToken);
        if (token.IsFailed)
        {
            _logger.LogInformation("Invalid refresh token: {token}", refreshToken);
            return Result.Fail("Invalid refresh token");
        }
        var userRoles = (await _userRepository.GetRolesByUserIdAsync(token.Value.UserId)).ToList();
        if (!userRoles.Any())
        {
            _logger.LogInformation("User not found, userid: {userid}", token.Value.UserId);
            return Result.Fail("No user roles found");
        }

        var userRoleEnums = userRoles.Select(x => RolesConvert.Convert(x.RoleName)).ToList();
        
        var jwt = GenerateJwt(token.Value.UserId, userRoleEnums);
        return new LoginResponse(jwt, token.Value.Token, token.Value.Expires);
    }
    
}