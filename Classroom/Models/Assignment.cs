using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class Assignment : EntityBase
{
    public int AssignmentId { get; set; }

    public Guid? AssignmentGuid { get; set; }

    public string Title { get; set; } = null!;

    public string? Instructions { get; set; }

    public string? Points { get; set; }

    public string? DueDate { get; set; }

    public string? DueTime { get; set; }

    public string? Topic { get; set; }

    public string? ScheduledFor { get; set; }

    public string? ClassName { get; set; }

    public string? Section { get; set; }

    public int? ClassId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Status { get; set; }

    public bool? AllowLateSubmissions { get; set; }

    public string? LateSubmissionPolicy { get; set; }

    public string? Color { get; set; }

    public virtual ICollection<AssignmentAttachment> AssignmentAttachments { get; set; } = new List<AssignmentAttachment>();

    public virtual Course? Class { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
