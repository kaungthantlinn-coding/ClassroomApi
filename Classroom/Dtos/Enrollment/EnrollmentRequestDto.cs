namespace Classroom.Dtos.Enrollment;

public class EnrollmentRequestDto
{
    public int EnrollmentRequestId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedById { get; set; }
    public string? ProcessedByName { get; set; }
    public string? RejectionReason { get; set; }
}

public class CreateEnrollmentRequestDto
{
    public int CourseId { get; set; }
    public string? Message { get; set; }
}

public class ProcessEnrollmentRequestDto
{
    public string Action { get; set; } = string.Empty; // Approve, Reject
    public string? RejectionReason { get; set; }
}

public class EnrollmentRequestResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public EnrollmentRequestDto? EnrollmentRequest { get; set; }
}

public class EnrollmentRequestsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<EnrollmentRequestDto> EnrollmentRequests { get; set; } = new List<EnrollmentRequestDto>();
}
