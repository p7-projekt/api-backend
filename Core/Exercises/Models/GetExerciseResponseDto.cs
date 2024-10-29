using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exercises.Models;

public class GetExerciseResponseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Solution {  get; set; } = string.Empty;
    public List<TestCaseEntity> TestCases { get; set; } = new();
}
