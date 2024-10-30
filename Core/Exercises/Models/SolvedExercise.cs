namespace Core.Exercises.Models;

public class SolvedExercise
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public bool Solved { get; set; }
}

