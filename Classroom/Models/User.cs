using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class User
{
    public int UserId { get; set; }

    public Guid? UserGuid { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<AnnouncementComment> AnnouncementComments { get; set; } = new List<AnnouncementComment>();

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<CourseMember> CourseMembers { get; set; } = new List<CourseMember>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
