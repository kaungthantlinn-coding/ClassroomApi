using Classroom.Dtos.Submission;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
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
}