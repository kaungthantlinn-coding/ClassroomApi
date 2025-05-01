using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class Material
{
    public int MaterialId { get; set; }

    public Guid? MaterialGuid { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Topic { get; set; }

    public string? ScheduledFor { get; set; }

    public string? ClassName { get; set; }

    public string? Section { get; set; }

    public int? ClassId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Color { get; set; }

    public virtual Course? Class { get; set; }

    public virtual ICollection<MaterialAttachment> MaterialAttachments { get; set; } = new List<MaterialAttachment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
