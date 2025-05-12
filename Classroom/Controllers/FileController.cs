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
    private readonly string[] _allowedFileExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip" };
    private readonly long _maxFileSize = 50 * 1024 * 1024; // 50 MB

    public FileController(IFileService fileService, ISubmissionService submissionService)
    {
        _fileService = fileService;
        _submissionService = submissionService;
    }

    /// <summary>
    /// Uploads a file for an assignment submission
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment</param>
    /// <param name="submissionId">Optional: The ID of an existing submission. If not provided, a new submission will be created.</param>
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
    public async Task<IActionResult> UploadSubmissionFile([FromForm] int assignmentId, [FromForm] int? submissionId, IFormFile file)
    {
        // Log the request parameters
        Console.WriteLine($"File Upload Request - AssignmentId: {assignmentId}, SubmissionId: {submissionId}, File: {(file != null ? file.FileName : "null")}");

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
            // If no submissionId is provided, create a new submission
            if (!submissionId.HasValue || submissionId.Value == 0)
            {
                var createSubmissionDto = new CreateSubmissionDto
                {
                    SubmissionContent = "File submission"
                };

                var submission = await _submissionService.CreateSubmissionAsync(assignmentId, createSubmissionDto, currentUserId);
                submissionId = submission.SubmissionId;
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
            var attachment = await _fileService.SaveSubmissionFileAsync(file, submissionId.Value);

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

}
