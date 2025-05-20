using System;

namespace Classroom.Dtos.Assignment;

public class CalendarAssignmentDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = null!;
    public string? DueDate { get; set; }
    public string? DueTime { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public string? Status { get; set; }
}
