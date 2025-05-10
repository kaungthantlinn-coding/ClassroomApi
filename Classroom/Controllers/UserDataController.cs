using Classroom.Dtos;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api/user/data")]
[ApiController]
[Authorize]
public class UserDataController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IUserService _userService;

    public UserDataController(ICourseService courseService, IUserService userService)
    {
        _courseService = courseService;
        _userService = userService;
    }

    // GET: api/user/data/class/{id}
    // Get user data for a specific class
    [HttpGet("class/{id}")]
    public async Task<IActionResult> GetUserClassData(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Use the user service to get class data
        var userData = await _userService.GetUserClassDataAsync(currentUserId, id);
        if (userData == null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        // Get detailed course information
        var courseDetail = await _courseService.GetCourseDetailAsync(id, currentUserId);

        // Return the user's data for this class
        return Ok(new
        {
            courseId = id,
            userId = currentUserId,
            courseDetail = courseDetail,
            userData = userData
        });
    }

    // POST: api/user/data/class/{id}
    // Save user data for a specific class
    [HttpPost("class/{id}")]
    public async Task<IActionResult> SaveUserClassData(int id, [FromBody] UserClassDataDto userClassDataDto)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Use the user service to save class data
        var result = await _userService.SaveUserClassDataAsync(currentUserId, id, userClassDataDto);
        if (!result)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        return Ok(new
        {
            success = true,
            message = "User class data saved successfully",
            courseId = id,
            userId = currentUserId
        });
    }
}
