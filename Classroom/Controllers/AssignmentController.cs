using Classroom.Dtos.Assignment;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    // GET: api/courses/{courseId}/assignments
    // List assignments in a course
    [HttpGet("courses/{courseId}/assignments")]
    public async Task<IActionResult> GetCourseAssignments(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var assignments = await _assignmentService.GetCourseAssignmentsAsync(courseId, currentUserId);
        return Ok(assignments);
    }

    // POST: api/courses/{courseId}/assignments
    // Create assignment (teacher only)
    [HttpPost("courses/{courseId}/assignments")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateAssignment(int courseId, [FromBody] CreateAssignmentDto createAssignmentDto)
    {
        // Validation removed as requested

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var assignment = await _assignmentService.CreateAssignmentAsync(courseId, createAssignmentDto, currentUserId);
            return CreatedAtAction(nameof(GetAssignmentById), new { id = assignment.AssignmentId }, assignment);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Course not found" });
        }
    }

    // GET: api/assignments/{id}
    // Get assignment by ID
    [HttpGet("assignments/{id}")]
    public async Task<IActionResult> GetAssignmentById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id, currentUserId);

        if (assignment is null)
        {
            return NotFound(new { message = "Assignment not found or you don't have access to it" });
        }

        return Ok(assignment);
    }

    // PUT: api/assignments/{id}
    // Update assignment (teacher only)
    [HttpPut("assignments/{id}")]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto updateAssignmentDto)
    {
        // Validation removed as requested

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var assignment = await _assignmentService.UpdateAssignmentAsync(id, updateAssignmentDto, currentUserId);

        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found or you are not authorized to update it" });
        }

        return Ok(assignment);
    }

    // DELETE: api/assignments/{id}
    // Delete assignment (teacher only)
    [HttpDelete("assignments/{id}")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _assignmentService.DeleteAssignmentAsync(id, currentUserId);

            if (!result)
            {
                return NotFound(new { message = "Assignment not found or you are not authorized to delete it" });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/calendar/assignments
    // Get assignments for calendar view across all courses the user is enrolled in
    [HttpGet("calendar/assignments")]
    public async Task<IActionResult> GetCalendarAssignments([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var assignments = await _assignmentService.GetCalendarAssignmentsAsync(
                startDate ?? string.Empty,
                endDate ?? string.Empty,
                currentUserId);

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}