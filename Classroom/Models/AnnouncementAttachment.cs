using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class AnnouncementAttachment
{
    public int AttachmentId { get; set; }

    public int AnnouncementId { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int? Size { get; set; }

    public DateTime? UploadDate { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;
}
