using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Infrastructure.Authentication.Models;

public class User
{
	public int Id { get; set; }
	public string? Email { get; set; }
	
	public string Name { get; set; } = string.Empty;
	public string? PasswordHash { get; set; } 
	public DateTime CreatedAt { get; set; }

	public bool Anonymous { get; set; }
	
}