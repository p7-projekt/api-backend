namespace Infrastructure.Authentication;

public class AuthConstants
{
	public static string Issuer = "Backender";
	public static string Audience = "Frontend";
	public static string JwtSecret = "JWT_KEY";
	public static int JwtExpirationInMinutes = 30;
}