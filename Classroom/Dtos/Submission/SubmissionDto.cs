namespace Classroom.Dtos.Submission;

public class SubmissionDto
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = null!;
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public decimal? Grade { get; set; }
    public string? Feedback { get; set; }
    public bool Graded { get; set; }
    public DateTime? GradedDate { get; set; }
    public string? SubmissionContent { get; set; }
}

public class CreateSubmissionDto
{
    public string? SubmissionContent { get; set; }
}

public class GradeSubmissionDto
{
    public decimal Grade { get; set; }
}

public class FeedbackSubmissionDto
{
    public string Feedback { get; set; } = null!;
}