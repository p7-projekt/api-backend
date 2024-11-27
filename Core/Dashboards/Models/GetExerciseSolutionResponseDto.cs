using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dashboards.Models
{
    public record GetExerciseSolutionResponseDto(
        string Title,
        string Description,
        string Solution,
        string Language,
        int Language_id
        );
}
