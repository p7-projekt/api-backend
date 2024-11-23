using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public record GetClassroomSessionDto(int Id, string Title, bool Active);
