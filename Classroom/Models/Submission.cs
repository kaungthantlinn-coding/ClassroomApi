using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public int UserId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public bool? Graded { get; set; }

    public DateTime? GradedDate { get; set; }

    public string? Content { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
