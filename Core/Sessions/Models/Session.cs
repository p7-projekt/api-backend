using Core.Exercises.Models;
using System.Globalization;

namespace Core.Sessions.Models;

public class Session
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    
    public DateTime ExpirationTimeUtc { get; set; }

    public string SessionCode { get; set; } = string.Empty;

    public List<int> Exercises { get; set; } = new (); // this needs to be changed when exercises are available

    public List<SolvedExercise> ExerciseDetails { get; set; } = new();
}

public static class SessionMapper
{
    public static Session ConvertToSession(this CreateSessionDto dto)
    {
        return new Session
        {
            Title = dto.Title,
            Description = dto.Description,
            ExpirationTimeUtc = DateTime.UtcNow.AddHours(dto.ExpiresInHours),
            Exercises = dto.ExerciseIds
        };
    }
    
    public static GetSessionsResponseDto ConvertToGetSessionsResponse(this Session session)
    {
        return new GetSessionsResponseDto(session.Id, session.Title, Math.Floor((session.ExpirationTimeUtc - DateTime.UtcNow).TotalSeconds).ToString(CultureInfo.InvariantCulture));
    }

    public static GetSessionResponseDto ConvertToGetResponse(this Session session)
    {
        return new GetSessionResponseDto(session.Title, session.Description, session.AuthorName,
           session.ExpirationTimeUtc, session.ExerciseDetails.Select(x => new GetExercisesResponseDto(x.ExerciseId, x.ExerciseTitle, x.Solved)).ToList());
    }
}
