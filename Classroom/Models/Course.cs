using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class Course : EntityBase
{
    public int CourseId { get; set; }

    public Guid? CourseGuid { get; set; }

    public string Name { get; set; } = null!;

    public string? Section { get; set; }

    public string? TeacherName { get; set; }

    public string? CoverImage { get; set; }

    public string EnrollmentCode { get; set; } = null!;

    public string? Subject { get; set; }

    public string? Room { get; set; }

    public string? ThemeColor { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<CourseMember> CourseMembers { get; set; } = new List<CourseMember>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
