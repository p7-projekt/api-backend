using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Infrastructure.Authentication.Models;

public class User
{
	public int Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public string RoleStr
	{
		set
		{
			switch (value)
			{
				case nameof(Roles.Instructor):
					Role = Roles.Instructor;
				break;
				case nameof(Roles.AnonymousUser):
					Role = Roles.AnonymousUser; 
				break;
			}
		}
	}

	public Roles Role { get; set; }
}