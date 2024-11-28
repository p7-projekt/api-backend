using Core.Exercises.Models;
using Core.Languages.Models;

namespace Core.Sessions.Models;

public record GetSessionResponseDto(
	string Title, 
	string? Description, 
	string Author, 
	DateTime SessionExpiresUtc, 
	List<SolvedExerciseDto> Exercises, 
	List<GetLanguagesResponseDto> Languages);


