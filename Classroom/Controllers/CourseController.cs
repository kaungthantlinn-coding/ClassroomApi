using Classroom.Dtos;
using Classroom.Dtos.Course;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    // Note: EnrollmentCode will be auto-generated if not provided
    // The auto-generated values will be returned in the response
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

    // GET: api/courses/generate-enrollment-code
    // Generate a unique enrollment code (teacher only)
    [HttpGet("generate-enrollment-code")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GenerateEnrollmentCode()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var code = await _courseService.GenerateEnrollmentCodeAsync();
        return Ok(new { code });
    }

    // GET: api/courses/{id}/enrollment-code
    // Get the enrollment code for a course (teacher only)
    [HttpGet("{id}/enrollment-code")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetCourseEnrollmentCode(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var course = await _courseService.GetCourseByIdAsync(id, currentUserId);

        if (course is null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        // Check if user is the teacher of the course
        var isTeacher = await _courseService.IsUserTeacherOfCourseAsync(id, currentUserId);
        if (!isTeacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to access the enrollment code" });
        }

        return Ok(new { enrollmentCode = course.EnrollmentCode });
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

    // POST: api/courses/{id}/regenerate-enrollment-code
    // Regenerate enrollment code for a course (teacher only)
    [HttpPost("{id}/regenerate-enrollment-code")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> RegenerateEnrollmentCode(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var newCode = await _courseService.RegenerateEnrollmentCodeAsync(id, currentUserId);

        if (newCode == null)
        {
            return NotFound(new { message = "Course not found or you are not authorized to regenerate its enrollment code" });
        }

        return Ok(new { enrollmentCode = newCode });
    }



    // GET: api/courses/{id}/detail
    // Get detailed information about a course
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetCourseDetail(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var courseDetail = await _courseService.GetCourseDetailAsync(id, currentUserId);

        if (courseDetail == null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        return Ok(courseDetail);
    }

    // GET: api/courses/guid/{guid}
    // Get a specific course by GUID
    [HttpGet("guid/{guid}")]
    public async Task<IActionResult> GetCourseByGuid(Guid guid)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var course = await _courseService.GetCourseByGuidAsync(guid, currentUserId);

        if (course == null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        return Ok(course);
    }

    // GET: api/courses/guid/{guid}/detail
    // Get detailed information about a course by GUID
    [HttpGet("guid/{guid}/detail")]
    public async Task<IActionResult> GetCourseDetailByGuid(Guid guid)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var courseDetail = await _courseService.GetCourseDetailByGuidAsync(guid, currentUserId);

        if (courseDetail == null)
        {
            return NotFound(new { message = "Course not found or you don't have access to it" });
        }

        return Ok(courseDetail);
    }

    // PUT: api/courses/theme
    // Update course theme (teacher only)
    [HttpPut("theme")]
    public async Task<IActionResult> UpdateCourseTheme([FromBody] CourseThemeDto themeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.UpdateCourseThemeAsync(themeDto, currentUserId);

        if (!result)
        {
            return NotFound(new { message = "Course not found or you are not authorized to update its theme" });
        }

        return Ok(new { message = "Course theme updated successfully" });
    }

    // POST: api/courses/enroll-by-code
    // Enroll in a course using just the enrollment code
    // This endpoint works for both students and teachers
    [HttpPost("enroll-by-code")]
    public async Task<IActionResult> EnrollByCode([FromBody] EnrollCourseDto enrollCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _courseService.EnrollInCourseByCodeAsync(enrollCourseDto.EnrollmentCode, currentUserId);

        if (!result)
        {
            return BadRequest(new { message = "Failed to enroll in course. Check if the course exists and the enrollment code is correct." });
        }

        return Ok(new { message = "Successfully enrolled in course" });
    }
}