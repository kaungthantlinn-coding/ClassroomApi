using Classroom.Dtos.Material;
using Classroom.Dtos.Submission;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

/// <summary>
/// Controller for handling file operations (upload, download, delete)
/// </summary>
[Route("api")]
[ApiController]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ISubmissionService _submissionService;
    private readonly IMaterialService _materialService;
    private readonly string[] _allowedFileExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip" };
    private readonly long _maxFileSize = 50 * 1024 * 1024; // 50 MB

    // Static lock object to ensure thread safety across multiple requests
    private static readonly object _submissionLock = new object();

    public FileController(IFileService fileService, ISubmissionService submissionService, IMaterialService materialService)
    {
        _fileService = fileService;
        _submissionService = submissionService;
        _materialService = materialService;
    }

    /// <summary>
    /// Uploads a file for an assignment submission
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment</param>
    /// <param name="submissionId">Optional: The ID of an existing submission. If not provided, a new submission will be created.</param>
    /// <param name="fileId">Optional: The ID of an existing file to replace. If not provided, a new file will be created.</param>
    /// <param name="file">The file to upload</param>
    /// <returns>Information about the uploaded file</returns>
    /// <response code="200">Returns the file information if upload is successful</response>
    /// <response code="400">If the file is null, too large, or has an invalid extension</response>
    /// <response code="403">If the user doesn't have permission to upload to this submission</response>
    /// <response code="404">If the assignment or submission is not found</response>
    /// <response code="500">If there's a server error during upload</response>
    [HttpPost("submissions/upload")]
    [RequestSizeLimit(52428800)] // 50 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)] // 50 MB
    public async Task<IActionResult> UploadSubmissionFile([FromForm] int assignmentId, [FromForm] int? submissionId, [FromForm] int? fileId, IFormFile file)
    {
        // Log the request parameters
        Console.WriteLine($"File Upload Request - AssignmentId: {assignmentId}, SubmissionId: {submissionId}, FileId: {fileId}, File: {(file != null ? file.FileName : "null")}");

        // Validate file
        if (file == null)
        {
            Console.WriteLine("File is null - returning BadRequest");
            return BadRequest(new { success = false, message = "No file uploaded" });
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            return BadRequest(new { success = false, message = "File size exceeds the maximum allowed size (50 MB)" });
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedFileExtensions.Contains(fileExtension))
        {
            return BadRequest(new
            {
                success = false,
                message = $"File type not allowed. Allowed types: {string.Join(", ", _allowedFileExtensions)}"
            });
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        Console.WriteLine($"Current User ID: {currentUserId}");

        try
        {
            // If no submissionId is provided, check if there's an existing submission for this assignment
            if (!submissionId.HasValue || submissionId.Value == 0)
            {
                // Add a lock to prevent race conditions when checking for existing submissions
                // and creating new ones if needed
                lock (_submissionLock)
                {
                    // Check if the user already has a submission for this assignment
                    // Use Task.Run to avoid deadlocks with the lock
                    var existingSubmissionTask = Task.Run(() => _submissionService.GetExistingSubmissionAsync(assignmentId, currentUserId)).Result;

                    if (existingSubmissionTask != null)
                    {
                        // Use the existing submission
                        Console.WriteLine($"Using existing submission ID: {existingSubmissionTask.SubmissionId}");
                        submissionId = existingSubmissionTask.SubmissionId;
                    }
                    else
                    {
                        // Create a new submission if none exists
                        var createSubmissionDto = new CreateSubmissionDto
                        {
                            SubmissionContent = "File submission"
                        };

                        // Use Task.Run to avoid deadlocks with the lock
                        var submissionTask = Task.Run(() => _submissionService.CreateSubmissionAsync(assignmentId, createSubmissionDto, currentUserId)).Result;
                        submissionId = submissionTask.SubmissionId;
                        Console.WriteLine($"Created new submission ID: {submissionId}");
                    }
                }
            }
            else
            {
                // Verify the user has access to this submission
                var submission = await _submissionService.GetSubmissionByIdAsync(submissionId.Value, currentUserId);
                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Submission not found or you don't have access to it" });
                }
            }

            // Save the file
            var attachment = await _fileService.SaveSubmissionFileAsync(file, submissionId.Value, fileId);

            // Return the file information
            return Ok(new SubmissionFilesResponseDto
            {
                Success = true,
                Files = new List<SubmissionFileResponseDto>
                {
                    new SubmissionFileResponseDto
                    {
                        AttachmentId = attachment.AttachmentId,
                        Name = attachment.Name,
                        Type = attachment.Type,
                        Size = attachment.Size ?? 0,
                        Url = attachment.Url,
                        UploadDate = attachment.UploadDate ?? DateTime.UtcNow
                    }
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error uploading file: {ex.Message}" });
        }
    }

    /// <summary>
    /// Downloads a submission file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to download</param>
    /// <returns>The file stream</returns>
    /// <response code="200">Returns the file if found and user has access</response>
    /// <response code="403">If the user doesn't have permission to access this file</response>
    /// <response code="404">If the file is not found</response>
    /// <response code="500">If there's a server error during download</response>
    [HttpGet("submissions/files/{fileId}")]
    public async Task<IActionResult> DownloadSubmissionFile(int fileId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var attachment = await _fileService.GetSubmissionFileByIdAsync(fileId);
            if (attachment == null)
            {
                return NotFound(new { success = false, message = $"File with ID {fileId} not found" });
            }

            // Check if user has access to this submission
            var submission = await _submissionService.GetSubmissionByIdAsync(attachment.SubmissionId, currentUserId);
            if (submission == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "You don't have permission to access this file" });
            }

            // Get the file stream
            var fileStream = await _fileService.GetSubmissionFileStreamAsync(fileId);

            // Return the file
            return File(fileStream, attachment.Type, attachment.Name);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error downloading file: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes a submission file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to delete</param>
    /// <returns>Success message if deleted</returns>
    /// <response code="200">Returns success message if file was deleted</response>
    /// <response code="403">If the user doesn't have permission to delete this file</response>
    /// <response code="404">If the file is not found</response>
    /// <response code="500">If there's a server error during deletion</response>
    [HttpDelete("submissions/files/{fileId}")]
    public async Task<IActionResult> DeleteSubmissionFile(int fileId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _fileService.DeleteSubmissionFileAsync(fileId, currentUserId);
            if (!result)
            {
                return NotFound(new { success = false, message = $"File with ID {fileId} not found" });
            }

            return Ok(new { success = true, message = "File deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error deleting file: {ex.Message}" });
        }
    }

    /// <summary>
    /// Uploads a file for a material
    /// </summary>
    /// <param name="materialId">The ID of the material</param>
    /// <param name="file">The file to upload</param>
    /// <returns>Information about the uploaded file</returns>
    /// <response code="200">Returns the file information if upload is successful</response>
    /// <response code="400">If the file is null, too large, or has an invalid extension</response>
    /// <response code="403">If the user doesn't have permission to upload to this material</response>
    /// <response code="404">If the material is not found</response>
    /// <response code="500">If there's a server error during upload</response>
    [HttpPost("materials/upload")]
    [RequestSizeLimit(52428800)] // 50 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)] // 50 MB
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UploadMaterialFile([FromForm] int materialId, IFormFile file)
    {
        // Log the request parameters
        Console.WriteLine($"Material File Upload Request - MaterialId: {materialId}, File: {(file != null ? file.FileName : "null")}");

        // Validate file
        if (file == null)
        {
            Console.WriteLine("File is null - returning BadRequest");
            return BadRequest(new { success = false, message = "No file uploaded", materialId = materialId, id = materialId });
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            return BadRequest(new { success = false, message = "File size exceeds the maximum allowed size (50 MB)", materialId = materialId, id = materialId });
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedFileExtensions.Contains(fileExtension))
        {
            return BadRequest(new
            {
                success = false,
                message = $"File type not allowed. Allowed types: {string.Join(", ", _allowedFileExtensions)}",
                materialId = materialId,
                id = materialId
            });
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        Console.WriteLine($"Current User ID: {currentUserId}");

        try
        {
            // Verify the material exists and user has access to it
            var material = await _materialService.GetMaterialByIdAsync(materialId, currentUserId);
            if (material == null)
            {
                return NotFound(new { success = false, message = "Material not found or you don't have access to it", materialId = materialId, id = materialId });
            }

            // Save the file
            var attachment = await _fileService.SaveMaterialFileAsync(file, materialId);

            // Return the material object with the file information
            // Get the material again to return its full details
            var updatedMaterial = await _materialService.GetMaterialByIdAsync(materialId, currentUserId);

            if (updatedMaterial == null)
            {
                return NotFound(new { success = false, message = "Material not found after file upload", materialId = materialId, id = materialId });
            }

            // The file should already be included in the updatedMaterial.Files collection
            // But let's make sure the file property is set for backward compatibility
            if (updatedMaterial.File == null)
            {
                updatedMaterial.File = new MaterialFileInfo
                {
                    AttachmentId = attachment.AttachmentId,
                    Name = attachment.Name,
                    Type = attachment.Type,
                    Size = (int)file.Length,
                    Url = attachment.Url,
                    UploadDate = DateTime.UtcNow
                };
            }

            // Return the material with file information
            return Ok(updatedMaterial);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message, materialId = materialId, id = materialId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, materialId = materialId, id = materialId });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error uploading file: {ex.Message}", materialId = materialId, id = materialId });
        }
    }

    /// <summary>
    /// Downloads a material file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to download</param>
    /// <returns>The file stream</returns>
    /// <response code="200">Returns the file if found and user has access</response>
    /// <response code="403">If the user doesn't have permission to access this file</response>
    /// <response code="404">If the file is not found</response>
    /// <response code="500">If there's a server error during download</response>
    [HttpGet("materials/files/{fileId}")]
    public async Task<IActionResult> DownloadMaterialFile(int fileId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var attachment = await _fileService.GetMaterialFileByIdAsync(fileId);
            if (attachment == null)
            {
                return NotFound(new { success = false, message = $"File with ID {fileId} not found" });
            }

            // Check if user has access to this material
            var material = await _materialService.GetMaterialByIdAsync(attachment.MaterialId, currentUserId);
            if (material == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "You don't have permission to access this file" });
            }

            // Get the file stream
            var fileStream = await _fileService.GetMaterialFileStreamAsync(fileId);

            // Return the file
            return File(fileStream, attachment.Type, attachment.Name);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error downloading file: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes a material file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to delete</param>
    /// <returns>Success message if deleted</returns>
    /// <response code="200">Returns success message if file was deleted</response>
    /// <response code="403">If the user doesn't have permission to delete this file</response>
    /// <response code="404">If the file is not found</response>
    /// <response code="500">If there's a server error during deletion</response>
    [HttpDelete("materials/files/{fileId}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteMaterialFile(int fileId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _fileService.DeleteMaterialFileAsync(fileId, currentUserId);
            if (!result)
            {
                return NotFound(new { success = false, message = $"File with ID {fileId} not found" });
            }

            return Ok(new { success = true, message = "File deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error deleting file: {ex.Message}" });
        }
    }
}
