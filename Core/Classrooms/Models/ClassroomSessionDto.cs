using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public record ClassroomSessionDto(
    string Title,
    string? Description,
    List<int> ExerciseIds,
    List<int> LanguageIds
    );
