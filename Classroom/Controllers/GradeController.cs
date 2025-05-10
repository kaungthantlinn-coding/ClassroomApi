using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class GradeController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public GradeController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    // GET: api/courses/{courseId}/grades
    // Get all grades for a course (teacher only)
    [HttpGet("courses/{courseId}/grades")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetCourseGrades(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var gradePage = await _submissionService.GetCourseGradePageAsync(courseId, currentUserId);
            return Ok(gradePage);
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

    // GET: api/courses/{courseId}/students/{studentId}/grades
    // Get grades for a specific student in a course (teacher or the student themselves)
    [HttpGet("courses/{courseId}/students/{studentId}/grades")]
    public async Task<IActionResult> GetStudentGrades(int courseId, int studentId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Only allow if the user is requesting their own grades or is a teacher
        if (currentUserId != studentId && userRole != "Teacher")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view these grades" });
        }

        try
        {
            var studentGrades = await _submissionService.GetStudentGradeDataAsync(courseId, studentId);

            if (studentGrades == null)
            {
                return NotFound($"Student with ID {studentId} not found in course with ID {courseId}");
            }

            return Ok(studentGrades);
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
}
