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
    public string? Color { get; set; }
    public string? TextColor { get; set; }
    public string? Subject { get; set; }
    public string? Room { get; set; }
}

public class CreateCourseDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public string? TeacherName { get; set; }

    public string? CoverImage { get; set; }

    [Required]
    public string EnrollmentCode { get; set; } = string.Empty;

    public string? Color { get; set; }

    public string? TextColor { get; set; }

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

    [Required]
    public string EnrollmentCode { get; set; } = string.Empty;

    public string? Color { get; set; }

    public string? TextColor { get; set; }

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

public class EnrollCourseDto
{
    [Required]
    public string EnrollmentCode { get; set; } = string.Empty;
}