using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface ITokenRepository
{
	Task InsertTokenAsync(RefreshToken token);
	Task<RefreshToken?> GetRefreshTokenByUserIdAsync(int userId);

	Task<RefreshToken?> GetRefreshTokenByRefreshTokenAsync(string refreshToken);
}