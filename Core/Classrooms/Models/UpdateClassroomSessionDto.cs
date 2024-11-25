using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public record UpdateClassroomSessionDto(int Id, string Title, string Description, bool Active, List<int> ExerciseIds);
