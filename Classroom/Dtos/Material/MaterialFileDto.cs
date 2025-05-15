using Microsoft.AspNetCore.Http;

namespace Classroom.Dtos.Material;

public class MaterialFileDto
{
    public int MaterialId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class MaterialFileResponseDto
{
    public int AttachmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

public class MaterialFilesResponseDto
{
    public bool Success { get; set; }
    public int MaterialId { get; set; }  // Add MaterialId for consistency
    public int Id { get; set; }          // Add Id for frontend compatibility
    public List<MaterialFileResponseDto> Files { get; set; } = new List<MaterialFileResponseDto>();
}
