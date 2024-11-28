using System.Text.Json.Serialization;

namespace Core.Exercises.Models;

public record SolvedExerciseDto(
	int Id, 
	string Name,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	bool? Solved);
