namespace Core.Solutions.Models;

public class Submission
{
	public int UserId { get; set; }
	
	public int SessionId { get; set; }
	
	public int ExerciseId { get; set; }
	
	public string? Solution { get; set; } 
	
	public int LanguageId { get; set; }
	
	public bool Solved { get; set; } = false;
}