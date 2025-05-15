using System;
using System.Collections.Generic;

namespace Classroom.Dtos.Material;

public class MaterialFileInfo
{
    public int AttachmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

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

    // Add file property to include attached files
    public MaterialFileInfo? File { get; set; }

    // Add files collection to include all attached files
    public List<MaterialFileInfo> Files { get; set; } = new List<MaterialFileInfo>();
}

public class CreateMaterialDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? ScheduledFor { get; set; }
    public string? Color { get; set; }
}

public class UpdateMaterialDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? ScheduledFor { get; set; }
    public string? Color { get; set; }
}