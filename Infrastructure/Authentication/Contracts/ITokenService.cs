using Infrastructure.Authentication.Models;

namespace Infrastructure.Authentication.Contracts;

public interface ITokenService
{
	string GenerateJwt(int userId, List<Roles> roles);
	string GenerateAnonymousUserJwt(int sessionLength);
}