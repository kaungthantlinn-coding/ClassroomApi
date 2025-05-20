namespace Classroom.Dtos.Notification;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}

public class SubmissionNotificationDto : NotificationDto
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class EnrollmentRequestNotificationDto : NotificationDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string EnrollmentStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
}

public enum NotificationType
{
    SubmissionCreated,
    SubmissionUpdated,
    GradeAdded,
    FeedbackAdded,
    CourseAnnouncement,
    NewAssignment,
    AssignmentDueSoon,
    EnrollmentRequest,
    EnrollmentApproved,
    EnrollmentRejected
}
