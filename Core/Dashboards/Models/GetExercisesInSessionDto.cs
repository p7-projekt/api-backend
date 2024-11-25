using System.Threading.Tasks;

namespace Core.Dashboards.Models;

public record GetExercisesInSessionResponseDto(
    string Title,
    int Id,
    int Solved,
    int Attempted,
    int[] UserIds,
    string[] Names
);

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