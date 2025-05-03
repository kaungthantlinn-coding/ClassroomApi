using Classroom.Dtos;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UserSettingsController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

 
    [HttpGet]
    [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserSettings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return Unauthorized(new { Success = false, Message = "User not authenticated" });
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { Success = false, Message = "User not found" });
        }

        var settings = new UserSettingsDto
        {
            Success = true,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            Role = user.Role,
            EmailNotifications = user.Email // For now, just use the email as the notification email
        };

        return Ok(settings);
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettingsDto settings)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return Unauthorized(new { Success = false, Message = "User not authenticated" });
        }

        var updateDto = new UpdateUserDto
        {
            Name = settings.Name,
            Email = settings.Email,
            Avatar = settings.Avatar,
            Role = settings.Role
        };

        var updatedUser = await _userService.UpdateUserAsync(id, updateDto);
        if (updatedUser == null)
        {
            return BadRequest(new { Success = false, Message = "Failed to update user settings" });
        }

        var response = new UserSettingsDto
        {
            Success = true,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Avatar = updatedUser.Avatar,
            Role = updatedUser.Role,
            EmailNotifications = updatedUser.Email // For now, just use the email as the notification email
        };

        return Ok(response);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return Unauthorized(new { Success = false, Message = "User not authenticated" });
        }

        var result = await _authService.ChangePasswordAsync(id, changePasswordDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
