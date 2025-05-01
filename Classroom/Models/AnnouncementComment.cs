using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class AnnouncementComment
{
    public int CommentId { get; set; }

    public int AnnouncementId { get; set; }

    public string Content { get; set; } = null!;

    public int AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorAvatar { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsPrivate { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;

    public virtual User Author { get; set; } = null!;
}
