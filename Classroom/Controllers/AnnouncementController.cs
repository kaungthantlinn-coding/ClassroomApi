using Classroom.Dtos.Announcement;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var announcement = await _announcementService.CreateAnnouncementAsync(courseId, createAnnouncementDto, currentUserId);
            return CreatedAtAction(nameof(GetAnnouncementById), new { id = announcement.AnnouncementId }, announcement);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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
            return Forbid(ex.Message);
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
            return Forbid(ex.Message);
        }
    }
}