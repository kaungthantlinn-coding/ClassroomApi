using System;

namespace Classroom.Dtos.Announcement;

public class AnnouncementDto
{
    public int AnnouncementId { get; set; }
    public Guid? AnnouncementGuid { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Content { get; set; } = null!;
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAnnouncementDto
{
    public string Content { get; set; } = null!;
}

public class UpdateAnnouncementDto
{
    public string Content { get; set; } = null!;
}