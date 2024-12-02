﻿using Core.Exercises.Models;
using Core.Languages.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public class GetClassroomSessionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Author { get; set; }
    public bool Active { get; set; }
    public List<SolvedExercise> Exercises { get; set; } = new ();
    public List<GetLanguagesResponseDto> Languages { get; set; } = new ();
}

