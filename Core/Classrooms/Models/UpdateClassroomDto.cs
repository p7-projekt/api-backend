﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public record UpdateClassroomDto(string Title, string Description, bool RegistrationOpen);
