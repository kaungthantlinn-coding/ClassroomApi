using Microsoft.AspNetCore.Http;

namespace Classroom.Dtos.Submission;

public class SubmissionFileDto
{
    public int SubmissionId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class SubmissionFileResponseDto
{
    public int AttachmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

public class SubmissionFilesResponseDto
{
    public bool Success { get; set; }
    public List<SubmissionFileResponseDto> Files { get; set; } = new List<SubmissionFileResponseDto>();
}
