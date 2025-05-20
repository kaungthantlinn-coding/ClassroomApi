using System.ComponentModel.DataAnnotations;

namespace Classroom.Dtos.Email;

public class CourseInvitationDto
{
    [Required]
    public int CourseId { get; set; }
    
    [Required]
    [EmailAddress]
    public string RecipientEmail { get; set; } = string.Empty;
    
    public string? CustomMessage { get; set; }
}

public class BulkCourseInvitationDto
{
    [Required]
    public int CourseId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one email address is required")]
    public List<string> RecipientEmails { get; set; } = new List<string>();
    
    public string? CustomMessage { get; set; }
}

public class EmailResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? FailedEmails { get; set; }
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
