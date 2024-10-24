using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface ITokenRepository
{
	Task InsertTokenAsync(RefreshToken token);

	Task<RefreshToken?> GetAccessTokenByRefreshTokenAsync(string refreshToken);
	Task DeleteRefreshTokenByUserIdAsync(int userId);
}