using System;
using System.Collections.Generic;

namespace Classroom.Dtos.Grade;

public class GradePageDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? Section { get; set; }
    public List<StudentGradeDto> StudentGrades { get; set; } = new List<StudentGradeDto>();
    public ClassMetricsDto ClassMetrics { get; set; } = new ClassMetricsDto();
    public GradeDistributionDto GradeDistribution { get; set; } = new GradeDistributionDto();
}

public class StudentGradeDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public decimal AssignmentAverage { get; set; }
    public decimal ParticipationScore { get; set; }
    public decimal FinalGrade { get; set; }
    public List<AssignmentGradeDto> AssignmentGrades { get; set; } = new List<AssignmentGradeDto>();
}

public class AssignmentGradeDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public string? Points { get; set; }
    public decimal? Grade { get; set; }
    public bool Submitted { get; set; }
    public bool Graded { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class ClassMetricsDto
{
    public decimal ClassAverage { get; set; }
    public decimal HighestGrade { get; set; }
    public decimal LowestGrade { get; set; }
    public int TotalStudents { get; set; }
    public int TotalGraded { get; set; }
}

public class GradeDistributionDto
{
    public int ACount { get; set; } // 90-100
    public int BCount { get; set; } // 80-89
    public int CCount { get; set; } // 70-79
    public int DCount { get; set; } // 60-69
    public int FCount { get; set; } // 0-59
}
