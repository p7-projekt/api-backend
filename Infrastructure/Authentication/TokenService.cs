using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public class TokenService
{
    private readonly ILogger<TokenService> _logger;
    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
    }
    public string GenerateJwt(int userId, Roles role)
    {
        var token = new JwtSecurityToken(
            issuer: AuthConstants.Issuer,
            audience: AuthConstants.Audience,
            claims: new List<Claim>
            {
                new Claim(ClaimTypes.UserData, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            },
            expires: DateTime.Now.AddMinutes(AuthConstants.JwtExpirationInMinutes),
            signingCredentials: GetSigningCredentials()
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateAnonymousUserJwt(int sessionLength)
    {
        var token = new JwtSecurityToken(
            issuer: AuthConstants.Issuer,
            audience: AuthConstants.Audience,
            claims: new List<Claim>
            {
                new Claim(ClaimTypes.UserData, AuthConstants.AnonymousUserId.ToString()),
                new Claim(ClaimTypes.Role, nameof(Roles.AnonymousUser))
            },
            expires: DateTime.Now.AddMinutes(sessionLength),
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
        }
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }
}