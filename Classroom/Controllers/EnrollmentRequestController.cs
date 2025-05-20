using Classroom.Dtos.Enrollment;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api/enrollment-requests")]
[ApiController]
[Authorize]
public class EnrollmentRequestController : ControllerBase
{
    private readonly IEnrollmentRequestService _enrollmentRequestService;
    private readonly ILogger<EnrollmentRequestController> _logger;

    public EnrollmentRequestController(
        IEnrollmentRequestService enrollmentRequestService,
        ILogger<EnrollmentRequestController> logger)
    {
        _enrollmentRequestService = enrollmentRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new enrollment request
    /// </summary>
    /// <param name="createEnrollmentRequestDto">The enrollment request data</param>
    /// <returns>The created enrollment request</returns>
    /// <response code="200">Returns the created enrollment request</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the course is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateEnrollmentRequest([FromBody] CreateEnrollmentRequestDto createEnrollmentRequestDto)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequest = await _enrollmentRequestService.CreateEnrollmentRequestAsync(createEnrollmentRequestDto, currentUserId);
            return Ok(new EnrollmentRequestResponseDto
            {
                Success = true,
                Message = "Enrollment request created successfully",
                EnrollmentRequest = enrollmentRequest
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enrollment request");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = $"Error creating enrollment request: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Gets an enrollment request by ID
    /// </summary>
    /// <param name="id">The ID of the enrollment request</param>
    /// <returns>The enrollment request</returns>
    /// <response code="200">Returns the enrollment request</response>
    /// <response code="403">If the user doesn't have permission to access the enrollment request</response>
    /// <response code="404">If the enrollment request is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollmentRequest(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequest = await _enrollmentRequestService.GetEnrollmentRequestByIdAsync(id, currentUserId);
            if (enrollmentRequest == null)
            {
                return NotFound(new EnrollmentRequestResponseDto
                {
                    Success = false,
                    Message = $"Enrollment request with ID {id} not found"
                });
            }

            return Ok(new EnrollmentRequestResponseDto
            {
                Success = true,
                EnrollmentRequest = enrollmentRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment request with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = $"Error getting enrollment request: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Gets all enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <returns>A list of enrollment requests</returns>
    /// <response code="200">Returns the list of enrollment requests</response>
    /// <response code="403">If the user doesn't have permission to access the enrollment requests</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetEnrollmentRequestsByCourse(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequests = await _enrollmentRequestService.GetEnrollmentRequestsByCourseIdAsync(courseId, currentUserId);
            return Ok(new EnrollmentRequestsResponseDto
            {
                Success = true,
                EnrollmentRequests = enrollmentRequests
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new EnrollmentRequestsResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment requests for course {courseId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestsResponseDto
            {
                Success = false,
                Message = $"Error getting enrollment requests: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Gets all pending enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <returns>A list of pending enrollment requests</returns>
    /// <response code="200">Returns the list of pending enrollment requests</response>
    /// <response code="403">If the user doesn't have permission to access the enrollment requests</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("course/{courseId}/pending")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetPendingEnrollmentRequestsByCourse(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequests = await _enrollmentRequestService.GetPendingEnrollmentRequestsByCourseIdAsync(courseId, currentUserId);
            return Ok(new EnrollmentRequestsResponseDto
            {
                Success = true,
                EnrollmentRequests = enrollmentRequests
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new EnrollmentRequestsResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting pending enrollment requests for course {courseId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestsResponseDto
            {
                Success = false,
                Message = $"Error getting pending enrollment requests: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Gets all enrollment requests for the current student
    /// </summary>
    /// <returns>A list of enrollment requests</returns>
    /// <response code="200">Returns the list of enrollment requests</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("my-requests")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyEnrollmentRequests()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequests = await _enrollmentRequestService.GetEnrollmentRequestsByStudentIdAsync(currentUserId);
            return Ok(new EnrollmentRequestsResponseDto
            {
                Success = true,
                EnrollmentRequests = enrollmentRequests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment requests for student {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestsResponseDto
            {
                Success = false,
                Message = $"Error getting enrollment requests: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Processes an enrollment request (approve or reject)
    /// </summary>
    /// <param name="id">The ID of the enrollment request</param>
    /// <param name="processEnrollmentRequestDto">The process data (approve/reject)</param>
    /// <returns>The processed enrollment request</returns>
    /// <response code="200">Returns the processed enrollment request</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="403">If the user doesn't have permission to process the enrollment request</response>
    /// <response code="404">If the enrollment request is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpPut("{id}/process")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ProcessEnrollmentRequest(int id, [FromBody] ProcessEnrollmentRequestDto processEnrollmentRequestDto)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var enrollmentRequest = await _enrollmentRequestService.ProcessEnrollmentRequestAsync(id, processEnrollmentRequestDto, currentUserId);
            if (enrollmentRequest == null)
            {
                return NotFound(new EnrollmentRequestResponseDto
                {
                    Success = false,
                    Message = $"Enrollment request with ID {id} not found"
                });
            }

            return Ok(new EnrollmentRequestResponseDto
            {
                Success = true,
                Message = $"Enrollment request {processEnrollmentRequestDto.Action.ToLower()}d successfully",
                EnrollmentRequest = enrollmentRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing enrollment request with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = $"Error processing enrollment request: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Cancels an enrollment request
    /// </summary>
    /// <param name="id">The ID of the enrollment request</param>
    /// <returns>Success message if canceled</returns>
    /// <response code="200">Returns success message if canceled</response>
    /// <response code="403">If the user doesn't have permission to cancel the enrollment request</response>
    /// <response code="404">If the enrollment request is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CancelEnrollmentRequest(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _enrollmentRequestService.CancelEnrollmentRequestAsync(id, currentUserId);
            if (!result)
            {
                return NotFound(new EnrollmentRequestResponseDto
                {
                    Success = false,
                    Message = $"Enrollment request with ID {id} not found"
                });
            }

            return Ok(new EnrollmentRequestResponseDto
            {
                Success = true,
                Message = "Enrollment request canceled successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error canceling enrollment request with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new EnrollmentRequestResponseDto
            {
                Success = false,
                Message = $"Error canceling enrollment request: {ex.Message}"
            });
        }
    }
}
