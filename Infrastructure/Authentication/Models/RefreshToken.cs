namespace Infrastructure.Authentication.Models;

public class RefreshToken
{
	public int Id { get; set; }
	public string Token { get; set; } = string.Empty;
	public DateTime Expires { get; set; }
	public DateTime CreatedAt { get; set; }
	public int UserId { get; set; }
}