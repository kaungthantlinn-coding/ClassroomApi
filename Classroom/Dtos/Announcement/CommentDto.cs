using System;

namespace Classroom.Dtos.Announcement;

public class CommentDto
{
    public int CommentId { get; set; }
    public int AnnouncementId { get; set; }
    public string Content { get; set; } = null!;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPrivate { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
    public bool IsPrivate { get; set; } = false;
}

public class UpdateCommentDto
{
    public string Content { get; set; } = null!;
    public bool IsPrivate { get; set; } = false;
}