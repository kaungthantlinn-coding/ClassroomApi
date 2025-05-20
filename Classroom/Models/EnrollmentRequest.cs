using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Models;

public class EnrollmentRequest
{
    [Key]
    public int EnrollmentRequestId { get; set; }
    
    [Required]
    public int CourseId { get; set; }
    
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [ForeignKey("StudentId")]
    public User? Student { get; set; }
    
    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    
    public DateTime? ProcessedAt { get; set; }
    
    public int? ProcessedById { get; set; }
    
    [ForeignKey("ProcessedById")]
    public User? ProcessedBy { get; set; }
    
    public string? RejectionReason { get; set; }
}
