namespace Core.Sessions.Models;

public record CreateSessionDto(
    string Title,
    string? Description,
    int ExpiresInHours,
    List<int> ExerciseIds,
    List<int> LanguageIds
    );