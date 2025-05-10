namespace Classroom.Dtos;

public class UserClassDataDto
{
    public int CourseId { get; set; }
    public int UserId { get; set; }
    
    // Add any additional properties that the frontend might be sending
    // These will depend on what data the frontend is trying to save
    public Dictionary<string, object>? CustomData { get; set; }
}
