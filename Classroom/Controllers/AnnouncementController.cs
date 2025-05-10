using Classroom.Dtos.Announcement;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class AnnouncementController(IAnnouncementService announcementService) : ControllerBase
{
    private readonly IAnnouncementService _announcementService = announcementService;

    // GET: api/courses/{courseId}/announcements
    // List announcements in a course
    [HttpGet("courses/{courseId}/announcements")]
    public async Task<IActionResult> GetCourseAnnouncements(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var announcements = await _announcementService.GetCourseAnnouncementsAsync(courseId, currentUserId);
        return Ok(announcements);
    }

    // POST: api/courses/{courseId}/announcements
    // Create announcement (teacher only)
    [HttpPost("courses/{courseId}/announcements")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateAnnouncement(int courseId, [FromBody] CreateAnnouncementDto createAnnouncementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            // Get the user's role from the claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if the user has the Teacher role
            if (userRole != "Teacher")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only teachers can create announcements" });
            }

            var announcement = await _announcementService.CreateAnnouncementAsync(courseId, createAnnouncementDto, currentUserId);
            return CreatedAtAction(nameof(GetAnnouncementById), new { id = announcement.AnnouncementId }, announcement);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // If the teacher is not found in the course, we'll add them automatically
            // This is handled in the AnnouncementService.CreateAnnouncementAsync method
            return NotFound(ex.Message);
        }
    }

    // POST: api/courses/{courseId}/announcements/force
    // Create announcement (teacher only) - force add teacher to course if not already enrolled
    [HttpPost("courses/{courseId}/announcements/force")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ForceCreateAnnouncement(int courseId, [FromBody] CreateAnnouncementDto createAnnouncementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Check if user has the Teacher role
        if (userRole != "Teacher")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only teachers can create announcements" });
        }

        try
        {
            // Get the course repository and user repository from the service provider
            var courseRepository = HttpContext.RequestServices.GetRequiredService<ICourseRepository>();
            var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            // Check if the course exists
            var course = await courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                return NotFound($"Course with ID {courseId} not found");
            }

            // Check if the user is already enrolled in the course
            var isEnrolled = await courseRepository.IsUserEnrolledAsync(courseId, currentUserId);

            // If not enrolled, add the user to the course as a teacher
            if (!isEnrolled)
            {
                // Get user details
                var user = await userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                {
                    return NotFound($"User with ID {currentUserId} not found");
                }

                // Add user to course as a teacher
                var courseMember = new CourseMember
                {
                    CourseId = courseId,
                    UserId = currentUserId,
                    Role = "Teacher"
                };

                await courseRepository.AddMemberAsync(courseMember);
            }

            // Create the announcement
            var announcement = await _announcementService.CreateAnnouncementAsync(courseId, createAnnouncementDto, currentUserId);
            return CreatedAtAction(nameof(GetAnnouncementById), new { id = announcement.AnnouncementId }, announcement);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Return detailed error information for debugging
            return StatusCode(500, new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.Message
            });
        }
    }

    // GET: api/announcements/{id}
    // Get announcement by ID
    [HttpGet("announcements/{id}")]
    public async Task<IActionResult> GetAnnouncementById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var announcement = await _announcementService.GetAnnouncementByIdAsync(id, currentUserId);

        if (announcement is null)
        {
            return NotFound($"Announcement with ID {id} not found or you don't have access to it");
        }

        return Ok(announcement);
    }

    // PUT: api/announcements/{id}
    // Update announcement (teacher only)
    [HttpPut("announcements/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementDto updateAnnouncementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var announcement = await _announcementService.UpdateAnnouncementAsync(id, updateAnnouncementDto, currentUserId);

            if (announcement is null)
            {
                return NotFound($"Announcement with ID {id} not found");
            }

            return Ok(announcement);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // DELETE: api/announcements/{id}
    // Delete announcement (teacher only)
    [HttpDelete("announcements/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _announcementService.DeleteAnnouncementAsync(id, currentUserId);

            if (!result)
            {
                return NotFound($"Announcement with ID {id} not found");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }
}