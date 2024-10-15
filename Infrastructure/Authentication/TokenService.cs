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
    public string GenerateJwt()
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY");
        if (key == null)
        {
            _logger.LogWarning("No jwt key environment variable found!");
        }
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "Backenden",
            audience: "Frontend",
            claims: new List<Claim>
            {
                new Claim(ClaimTypes.UserData, "1"),
                new Claim(ClaimTypes.Role, nameof(Roles.AnonymousUser))
            },
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}