using System.Threading.Tasks;

namespace Core.Dashboards.Models;

public class GetExercisesInSessionResponseDto
{
    public required string Title { get; set; }
    public int Id { get; set; }
    public int Solved { get; set; }
    public int Attempted { get; set; }
    public required int[] UserIds { get; set; }
    public required string[] Names { get; set; }
};

public record UserDetailDto(
    int Id,
    string Name
);

public record GetExercisesAndUserDetailsInSessionResponseDto(
    string Title,
    int Id,
    int Solved,
    int Attemped,
    List<UserDetailDto> UserDetails
);
public record GetExercisesInSessionCombinedInfo(
    int Participants,
    List<GetExercisesAndUserDetailsInSessionResponseDto> ExerciseDetails
);