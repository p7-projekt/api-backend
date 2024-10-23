using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface ITokenService : IAnonTokenService
{
	string GenerateJwt(int userId, List<Roles> roles);
	Task<Result<RefreshToken>> GenerateRefreshToken(int userId);
	Task<Result<LoginResponse>> GenerateJwtFromRefreshToken(RefreshDto refreshToken);
}