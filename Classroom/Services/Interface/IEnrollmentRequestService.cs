using Classroom.Dtos.Enrollment;

namespace Classroom.Services.Interface;

public interface IEnrollmentRequestService
{
    /// <summary>
    /// Creates a new enrollment request
    /// </summary>
    /// <param name="createEnrollmentRequestDto">The enrollment request data</param>
    /// <param name="studentId">The ID of the student making the request</param>
    /// <returns>The created enrollment request</returns>
    Task<EnrollmentRequestDto> CreateEnrollmentRequestAsync(CreateEnrollmentRequestDto createEnrollmentRequestDto, int studentId);
    
    /// <summary>
    /// Gets an enrollment request by ID
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <param name="userId">The ID of the user requesting the enrollment request</param>
    /// <returns>The enrollment request if found and the user has access, null otherwise</returns>
    Task<EnrollmentRequestDto?> GetEnrollmentRequestByIdAsync(int enrollmentRequestId, int userId);
    
    /// <summary>
    /// Gets all enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="teacherId">The ID of the teacher requesting the enrollment requests</param>
    /// <returns>A list of enrollment requests</returns>
    Task<List<EnrollmentRequestDto>> GetEnrollmentRequestsByCourseIdAsync(int courseId, int teacherId);
    
    /// <summary>
    /// Gets all enrollment requests for a student
    /// </summary>
    /// <param name="studentId">The ID of the student</param>
    /// <returns>A list of enrollment requests</returns>
    Task<List<EnrollmentRequestDto>> GetEnrollmentRequestsByStudentIdAsync(int studentId);
    
    /// <summary>
    /// Gets all pending enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="teacherId">The ID of the teacher requesting the enrollment requests</param>
    /// <returns>A list of pending enrollment requests</returns>
    Task<List<EnrollmentRequestDto>> GetPendingEnrollmentRequestsByCourseIdAsync(int courseId, int teacherId);
    
    /// <summary>
    /// Processes an enrollment request (approve or reject)
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <param name="processEnrollmentRequestDto">The process data (approve/reject)</param>
    /// <param name="teacherId">The ID of the teacher processing the request</param>
    /// <returns>The processed enrollment request</returns>
    Task<EnrollmentRequestDto?> ProcessEnrollmentRequestAsync(int enrollmentRequestId, ProcessEnrollmentRequestDto processEnrollmentRequestDto, int teacherId);
    
    /// <summary>
    /// Cancels an enrollment request
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <param name="studentId">The ID of the student canceling the request</param>
    /// <returns>True if the enrollment request was canceled, false otherwise</returns>
    Task<bool> CancelEnrollmentRequestAsync(int enrollmentRequestId, int studentId);
    
    /// <summary>
    /// Checks if a student has a pending enrollment request for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="studentId">The ID of the student</param>
    /// <returns>True if the student has a pending enrollment request, false otherwise</returns>
    Task<bool> HasPendingEnrollmentRequestAsync(int courseId, int studentId);
}
