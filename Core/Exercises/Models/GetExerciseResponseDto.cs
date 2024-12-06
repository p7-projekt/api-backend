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
    public List<string> InputParameterType { get; set; } = new();
    public List<string> OutputParameterType { get; set; } = new();
    public List<TestcaseDto> TestCases { get; set; } = new();

    public int LanguageId { get; set; }
}
