using Core.Exercises.Models;

namespace Core.Sessions.Models;

public record GetSessionResponseDto(string Title, string? Description, string Author, DateTime SessionExpiresUtc, List<SolvedExerciseDto> Exercises);


