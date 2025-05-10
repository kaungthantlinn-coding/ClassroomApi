using System.ComponentModel.DataAnnotations;

namespace Classroom.Dtos.Course;

public class CourseDto
{
    public int CourseId { get; set; }
    public Guid? CourseGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? TeacherName { get; set; }
    public string? CoverImage { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Room { get; set; }
    public string? ThemeColor { get; set; }
}

public class CreateCourseDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public string? TeacherName { get; set; }

    public string? CoverImage { get; set; }

    // EnrollmentCode is now optional as it will be auto-generated
    public string? EnrollmentCode { get; set; }

    public string? Subject { get; set; }

    public string? Room { get; set; }
}

public class UpdateCourseDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public string? TeacherName { get; set; }

    public string? CoverImage { get; set; }

    // EnrollmentCode is optional as it's auto-generated on creation
    public string? EnrollmentCode { get; set; }

    public string? Subject { get; set; }

    public string? Room { get; set; }
}

public class CourseMemberDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CourseDetailDto : CourseDto
{
    public int TotalMembers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalAnnouncements { get; set; }
    public int TotalAssignments { get; set; }
    public int TotalMaterials { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public List<CourseMemberDto> RecentMembers { get; set; } = new List<CourseMemberDto>();
}

public class EnrollCourseDto
{
    [Required]
    public string EnrollmentCode { get; set; } = string.Empty;
}