using System;

namespace Classroom.Dtos.Assignment;

public class AssignmentDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = null!;
    public string? Instructions { get; set; }
    public string? Points { get; set; }
    public string? DueDate { get; set; }
    public string? DueTime { get; set; }
    public string? Topic { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Status { get; set; }
    public bool? AllowLateSubmissions { get; set; }
    public string? LateSubmissionPolicy { get; set; }
}

public class CreateAssignmentDto
{
    public string? Title { get; set; }
    public string? Instructions { get; set; }
    public string? Points { get; set; }
    public string? DueDate { get; set; }
    public string? DueTime { get; set; }
    public string? Topic { get; set; }
    public string? Status { get; set; }
    public bool? AllowLateSubmissions { get; set; }
    public string? LateSubmissionPolicy { get; set; }
}

public class UpdateAssignmentDto
{
    public string? Title { get; set; }
    public string? Instructions { get; set; }
    public string? Points { get; set; }
    public string? DueDate { get; set; }
    public string? DueTime { get; set; }
    public string? Topic { get; set; }
    public string? Status { get; set; }
    public bool? AllowLateSubmissions { get; set; }
    public string? LateSubmissionPolicy { get; set; }
}