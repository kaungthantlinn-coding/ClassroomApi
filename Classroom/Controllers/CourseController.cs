using Classroom.Dtos;
using Classroom.Dtos.Course;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api/courses")]
[ApiController]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: api/courses
    // List all courses
    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // If user is a teacher, return all courses, otherwise return only enrolled courses
        if (userRole == "Teacher")
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }
        else
        {
            var courses = await _courseService.GetUserCoursesAsync(currentUserId);
            return Ok(courses);
        }
    }

    // POST: api/courses
    // Create a new course (teacher only)
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var course = await _courseService.CreateCourseAsync(createCourseDto, currentUserId);

        return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, course);
    }

    // GET: api/courses/{id}
    // Get course by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var course = await _courseService.GetCourseByIdAsync(id, currentUserId);

        if (course is null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        return Ok(course);
    }

    // PUT: api/courses/{id}
    // Update course (teacher only)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var course = await _courseService.UpdateCourseAsync(id, updateCourseDto, currentUserId);

        if (course == null)
        {
            return NotFound(new { message = "Course not found or you are not authorized to update it" });
        }

        return Ok(course);
    }

    // DELETE: api/courses/{id}
    // Delete course (teacher only)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.DeleteCourseAsync(id, currentUserId);

        if (!result)
        {
            return NotFound(new { message = "Course not found or you are not authorized to delete it" });
        }

        return NoContent();
    }

    // POST: api/courses/{id}/enroll
    // Enroll in a course (student only)
    [HttpPost("{id}/enroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> EnrollInCourse(int id, [FromBody] EnrollCourseDto enrollCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.EnrollInCourseAsync(id, enrollCourseDto.EnrollmentCode, currentUserId);

        if (!result)
        {
            return BadRequest(new { message = "Failed to enroll in course. Check if the course exists and the enrollment code is correct." });
        }

        return Ok(new { message = "Successfully enrolled in course" });
    }

    // POST: api/courses/{id}/unenroll
    // Unenroll from a course (student only)
    [HttpPost("{id}/unenroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UnenrollFromCourse(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.UnenrollFromCourseAsync(id, currentUserId);

        if (!result)
        {
            return BadRequest(new { message = "Failed to unenroll from course. Check if you are enrolled as a student." });
        }

        return Ok(new { message = "Successfully unenrolled from course" });
    }

    // GET: api/courses/{id}/members
    // List members of a course
    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetCourseMembers(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var members = await _courseService.GetCourseMembersAsync(id, currentUserId);

        return Ok(members);
    }

    // DELETE: api/courses/{id}/members/{userId}
    // Remove member from course (teacher only)
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMemberFromCourse(int id, int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.RemoveMemberFromCourseAsync(id, userId, currentUserId);

        if (!result)
        {
            return BadRequest(new { message = "Failed to remove member from course. Check if you are the teacher of this course." });
        }

        return NoContent();
    }
}