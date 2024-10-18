namespace Infrastructure.Authentication.Models;

public enum Roles
{
	Instructor,
	AnonymousUser,
	None
}

public class RolesConvert
{
	public static Roles Convert(string role)
	{
		var currentRole = Roles.None;
		switch (role)
		{
			case nameof(Roles.Instructor):
				currentRole = Roles.Instructor;
				break;
			case nameof(Roles.AnonymousUser):
				currentRole = Roles.AnonymousUser;
				break;
		}

		return currentRole;
	}
}