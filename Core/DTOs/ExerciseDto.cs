using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public record ExerciseDto(
        string Name, 
        string Description, 
        string Solution, 
        string[] InputParameterType, 
        string[] OutputParamaterType, 
        List<(string[], string[])> Testcases);
}
