using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Models;

public abstract class EntityBase
{
    public bool IsDeleted { get; set; } = false;

    [NotMapped] // This property is not mapped to a database column
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [NotMapped] // This property is not mapped to a database column
    public DateTime? LastUpdatedDate { get; set; }
}
