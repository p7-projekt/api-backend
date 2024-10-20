namespace Core.Shared.Contracts;

public interface IAnonTokenService
{
    string GenerateAnonymousUserJwt(int sessionLength, int userId);
}