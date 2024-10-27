using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exercises.Models;

public record ExerciseSubmissionDto (
    string Solution,
    string[] InputParameterType,
    string[] OutputParamaterType,
    List<Testcase> Testcases);
