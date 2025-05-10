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
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    // List all users (teacher only)
    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/users/{id}
    // Get user by ID (self or teacher)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        // Check if user is authorized to access this resource
        // Only allow if the user is requesting their own data or is a teacher
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id && userRole != "Teacher")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to access this user's data" });
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    // PUT: api/users/{id}
    // Update user (self or teacher)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if user is authorized to update this resource
        // Only allow if the user is updating their own data or is a teacher
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id && userRole != "Teacher")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to update this user's data" });
        }

        // Prevent non-teachers from changing roles
        if (userRole != "Teacher" && !string.IsNullOrEmpty(updateUserDto.Role))
        {
            return BadRequest(new { message = "You are not authorized to change roles" });
        }

        var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
        if (updatedUser == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(updatedUser);
    }
}