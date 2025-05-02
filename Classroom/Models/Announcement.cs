using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class Announcement : EntityBase
{
    public int AnnouncementId { get; set; }

    public Guid? AnnouncementGuid { get; set; }

    public int ClassId { get; set; }

    public string Content { get; set; } = null!;

    public int AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorAvatar { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AnnouncementAttachment> AnnouncementAttachments { get; set; } = new List<AnnouncementAttachment>();

    public virtual ICollection<AnnouncementComment> AnnouncementComments { get; set; } = new List<AnnouncementComment>();

    public virtual User Author { get; set; } = null!;

    public virtual Course Class { get; set; } = null!;
}
