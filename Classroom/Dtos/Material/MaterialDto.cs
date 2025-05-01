using System;

namespace Classroom.Dtos.Material;

public class MaterialDto
{
    public int MaterialId { get; set; }
    public Guid? MaterialGuid { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? ScheduledFor { get; set; }
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Section { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Color { get; set; }
}

public class CreateMaterialDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? ScheduledFor { get; set; }
    public string? Color { get; set; }
}

public class UpdateMaterialDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? ScheduledFor { get; set; }
    public string? Color { get; set; }
}