using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Classroom.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; } = false;
    
    // The user who should receive this notification
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    // Optional related entities
    public int? CourseId { get; set; }
    
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    
    public int? AssignmentId { get; set; }
    
    [ForeignKey("AssignmentId")]
    public Assignment? Assignment { get; set; }
    
    public int? SubmissionId { get; set; }
    
    [ForeignKey("SubmissionId")]
    public Submission? Submission { get; set; }
    
    // Store additional data as JSON
    public string? DataJson { get; set; }
    
    [NotMapped]
    public Dictionary<string, object> Data
    {
        get => string.IsNullOrEmpty(DataJson) 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(DataJson) ?? new Dictionary<string, object>();
        set => DataJson = JsonSerializer.Serialize(value);
    }
}
