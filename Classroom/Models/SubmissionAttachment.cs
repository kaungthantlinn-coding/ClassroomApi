using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class SubmissionAttachment
{
    public int AttachmentId { get; set; }

    public int SubmissionId { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int? Size { get; set; }

    public DateTime? UploadDate { get; set; }

    public virtual Submission Submission { get; set; } = null!;
}
