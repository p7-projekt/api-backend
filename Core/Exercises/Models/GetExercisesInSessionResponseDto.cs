using System.Threading.Tasks;

namespace Core.Exercises.Models;

public record GetExercisesInSessionResponseDto(
    int Id,
    string Solved,
    string Attempted,
    List<int> UserIds
);