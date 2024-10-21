using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.DTOs
{
    public record ExerciseDto(
        string Name,
        string Description,
        string Solution,
        string[] InputParameterType,
        string[] OutputParamaterType,
        List<Testcase> Testcases);

    public record Testcase(string[] inputParams, string[] outputParams);
}
