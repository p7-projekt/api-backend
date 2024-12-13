namespace Core.Shared;

public enum Roles
{
	Instructor,
	Student,
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
			case nameof(Roles.Student):
				currentRole = Roles.Student;
				break;
		}

		return currentRole;
	}
}