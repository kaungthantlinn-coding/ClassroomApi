using Classroom.Dtos.Submission;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly IFileService _fileService;

    public SubmissionController(ISubmissionService submissionService, IFileService fileService)
    {
        _submissionService = submissionService;
        _fileService = fileService;
    }

    // GET: api/assignments/{assignmentId}/submissions
    // List submissions for an assignment (teacher only)
    [HttpGet("assignments/{assignmentId}/submissions")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetAssignmentSubmissions(int assignmentId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var submissions = await _submissionService.GetAssignmentSubmissionsAsync(assignmentId, currentUserId);
        return Ok(submissions);
    }

    // POST: api/assignments/{assignmentId}/submissions
    // Submit assignment (student only)
    [HttpPost("assignments/{assignmentId}/submissions")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateSubmission(int assignmentId, [FromBody] CreateSubmissionDto createSubmissionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var submission = await _submissionService.CreateSubmissionAsync(assignmentId, createSubmissionDto, currentUserId);
            return CreatedAtAction(nameof(GetSubmissionById), new { id = submission.SubmissionId }, submission);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/submissions/{id}
    // Get submission by ID
    [HttpGet("submissions/{id}")]
    public async Task<IActionResult> GetSubmissionById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var submission = await _submissionService.GetSubmissionByIdAsync(id, currentUserId);

        if (submission is null)
        {
            return NotFound($"Submission with ID {id} not found or you don't have access to it");
        }

        return Ok(submission);
    }

    // PUT: api/submissions/{id}/grade
    // Grade submission (teacher only)
    [HttpPut("submissions/{id}/grade")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionDto gradeSubmissionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var submission = await _submissionService.GradeSubmissionAsync(id, gradeSubmissionDto, currentUserId);

            if (submission is null)
            {
                return NotFound($"Submission with ID {id} not found or you don't have access to it");
            }

            return Ok(submission);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // PUT: api/submissions/{id}/feedback
    // Add or update feedback (teacher only)
    [HttpPut("submissions/{id}/feedback")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> AddFeedback(int id, [FromBody] FeedbackSubmissionDto feedbackSubmissionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var submission = await _submissionService.AddFeedbackAsync(id, feedbackSubmissionDto, currentUserId);

            if (submission is null)
            {
                return NotFound($"Submission with ID {id} not found or you don't have access to it");
            }

            return Ok(submission);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // DELETE: api/submissions/{id}/unsubmit
    // Unsubmit a submission (student only)
    [HttpDelete("submissions/{id}/unsubmit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UnsubmitSubmission(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var submission = await _submissionService.UnsubmitSubmissionAsync(id, currentUserId);

            if (submission is null)
            {
                return NotFound($"Submission with ID {id} not found or you don't have access to it");
            }

            // Return a success response with the updated/deleted submission info
            return Ok(submission);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/assignments/{assignmentId}/submissions/purge
    // Purge all submissions for an assignment (student can purge their own, teacher can purge any)
    [HttpDelete("assignments/{assignmentId}/submissions/purge")]
    public async Task<IActionResult> PurgeSubmissions(int assignmentId, [FromQuery] int? studentId = null)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // If studentId is not provided, use the current user's ID
        int targetStudentId = studentId ?? currentUserId;

        // Students can only purge their own submissions
        if (userRole == "Student" && targetStudentId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only purge your own submissions" });
        }

        // Teachers can purge any student's submissions
        if (userRole != "Teacher" && targetStudentId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You don't have permission to purge other students' submissions" });
        }

        try
        {
            var result = await _submissionService.PurgeSubmissionsAsync(assignmentId, targetStudentId, currentUserId);
            return Ok(new { success = true, message = "Submissions purged successfully", purgedCount = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }



    /// <summary>
    /// Gets all files for a submission
    /// </summary>
    /// <param name="submissionId">The ID of the submission</param>
    /// <param name="studentId">Optional: The ID of the student who owns the submission</param>
    /// <returns>List of files in the submission</returns>
    /// <response code="200">Returns the list of files if found and user has access</response>
    /// <response code="403">If the user doesn't have permission to access this submission</response>
    /// <response code="404">If the submission is not found</response>
    [HttpGet("submissions/{submissionId}/files")]
    public async Task<IActionResult> GetSubmissionFiles(int submissionId, [FromQuery] int? studentId = null)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        try
        {
            // Verify the user has access to this submission
            var submission = await _submissionService.GetSubmissionByIdAsync(submissionId, currentUserId);
            if (submission == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "You don't have permission to access this submission" });
            }

            // Get all files for the submission
            var files = await _fileService.GetSubmissionFilesAsync(submissionId);

            // Map to response DTOs
            var fileResponses = files.Select(f => new SubmissionFileResponseDto
            {
                AttachmentId = f.AttachmentId,
                Name = f.Name,
                Type = f.Type,
                Size = f.Size ?? 0,
                Url = f.Url,
                UploadDate = f.UploadDate ?? DateTime.UtcNow
            }).ToList();

            return Ok(new SubmissionFilesResponseDto
            {
                Success = true,
                Files = fileResponses
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error retrieving files: {ex.Message}" });
        }
    }
}