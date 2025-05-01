using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class CourseMember
{
    public int CourseId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!; 

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
