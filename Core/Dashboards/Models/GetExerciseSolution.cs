using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dashboards.Models
{
    public record GetExerciseSolution(
        string title,
        string description,
        string solution
        );
}
