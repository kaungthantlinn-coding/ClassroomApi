using Classroom.Dtos.Email;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api/courses")]
[ApiController]
[Authorize]
public class CourseInvitationController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ICourseService _courseService;
    private readonly IValidationService _validationService;
    private readonly ILogger<CourseInvitationController> _logger;

    public CourseInvitationController(
        IEmailService emailService,
        ICourseService courseService,
        IValidationService validationService,
        ILogger<CourseInvitationController> logger)
    {
        _emailService = emailService;
        _courseService = courseService;
        _validationService = validationService;
        _logger = logger;
    }

    // POST: api/courses/{id}/invite
    // Send an invitation email to a student (teacher only)
    [HttpPost("{id}/invite")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> InviteStudent(int id, [FromBody] CourseInvitationDto invitationDto)
    {
        // Validate the invitation DTO
        var validationResult = _validationService.ValidateCourseInvitation(invitationDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Ensure the course ID in the path matches the one in the DTO
        if (id != invitationDto.CourseId)
        {
            return BadRequest(new { message = "Course ID in the path does not match the one in the request body" });
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if user is a teacher of the course
        var isTeacher = await _courseService.IsUserTeacherOfCourseAsync(id, currentUserId);
        if (!isTeacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to send invitations for this course" });
        }

        // Validate email format
        if (!await _emailService.ValidateEmailAsync(invitationDto.RecipientEmail))
        {
            return BadRequest(new { message = "Invalid email format" });
        }

        // Send the invitation
        var result = await _emailService.SendCourseInvitationAsync(invitationDto);
        if (!result)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to send invitation email" });
        }

        return Ok(new { message = $"Invitation sent to {invitationDto.RecipientEmail}" });
    }

    // POST: api/courses/{id}/invite-bulk
    // Send invitation emails to multiple students (teacher only)
    [HttpPost("{id}/invite-bulk")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> InviteMultipleStudents(int id, [FromBody] BulkCourseInvitationDto bulkInvitationDto)
    {
        // Validate the bulk invitation DTO
        var validationResult = _validationService.ValidateBulkCourseInvitation(bulkInvitationDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Ensure the course ID in the path matches the one in the DTO
        if (id != bulkInvitationDto.CourseId)
        {
            return BadRequest(new { message = "Course ID in the path does not match the one in the request body" });
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if user is a teacher of the course
        var isTeacher = await _courseService.IsUserTeacherOfCourseAsync(id, currentUserId);
        if (!isTeacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to send invitations for this course" });
        }

        // Process each email
        var failedEmails = new List<string>();
        var successCount = 0;

        foreach (var email in bulkInvitationDto.RecipientEmails)
        {
            // Validate email format
            if (!await _emailService.ValidateEmailAsync(email))
            {
                failedEmails.Add($"{email} (invalid format)");
                continue;
            }

            // Create individual invitation
            var invitation = new CourseInvitationDto
            {
                CourseId = id,
                RecipientEmail = email,
                CustomMessage = bulkInvitationDto.CustomMessage
            };

            // Send the invitation
            var result = await _emailService.SendCourseInvitationAsync(invitation);
            if (!result)
            {
                failedEmails.Add($"{email} (sending failed)");
            }
            else
            {
                successCount++;
            }
        }

        // Return appropriate response based on results
        if (failedEmails.Count == 0)
        {
            return Ok(new { message = $"Successfully sent {successCount} invitations" });
        }
        else if (successCount == 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { 
                message = "Failed to send any invitations", 
                failedEmails 
            });
        }
        else
        {
            return Ok(new { 
                message = $"Sent {successCount} invitations with {failedEmails.Count} failures", 
                failedEmails 
            });
        }
    }
}
