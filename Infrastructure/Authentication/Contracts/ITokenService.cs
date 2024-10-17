using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface ITokenService
{
	string GenerateJwt(int userId, List<Roles> roles);
	string GenerateAnonymousUserJwt(int sessionLength);
	Task<Result<RefreshToken>> GenerateRefreshToken(int userId);
	Task<Result<LoginResponse>> GenerateJwtFromRefreshToken(RefreshDto refreshToken);
}