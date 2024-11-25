using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Models;

public class GetClassroomResponseDto
{
    public GetClassroomResponseDto() { }
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Roomcode { get; set; } = string.Empty;
    public bool IsOpen { get; set; } 
    public int TotalStudents { get; set; }
    public List<GetClassroomSessionDto> Sessions { get; set; } = new();
}
