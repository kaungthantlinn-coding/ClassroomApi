using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IEnrollmentRequestRepository
{
    /// <summary>
    /// Creates a new enrollment request
    /// </summary>
    /// <param name="enrollmentRequest">The enrollment request to create</param>
    /// <returns>The created enrollment request</returns>
    Task<EnrollmentRequest> CreateEnrollmentRequestAsync(EnrollmentRequest enrollmentRequest);
    
    /// <summary>
    /// Gets an enrollment request by ID
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <returns>The enrollment request if found, null otherwise</returns>
    Task<EnrollmentRequest?> GetEnrollmentRequestByIdAsync(int enrollmentRequestId);
    
    /// <summary>
    /// Gets all enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <returns>A list of enrollment requests</returns>
    Task<List<EnrollmentRequest>> GetEnrollmentRequestsByCourseIdAsync(int courseId);
    
    /// <summary>
    /// Gets all enrollment requests for a student
    /// </summary>
    /// <param name="studentId">The ID of the student</param>
    /// <returns>A list of enrollment requests</returns>
    Task<List<EnrollmentRequest>> GetEnrollmentRequestsByStudentIdAsync(int studentId);
    
    /// <summary>
    /// Gets all pending enrollment requests for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <returns>A list of pending enrollment requests</returns>
    Task<List<EnrollmentRequest>> GetPendingEnrollmentRequestsByCourseIdAsync(int courseId);
    
    /// <summary>
    /// Gets an enrollment request by course ID and student ID
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="studentId">The ID of the student</param>
    /// <returns>The enrollment request if found, null otherwise</returns>
    Task<EnrollmentRequest?> GetEnrollmentRequestByCourseAndStudentAsync(int courseId, int studentId);
    
    /// <summary>
    /// Updates an enrollment request
    /// </summary>
    /// <param name="enrollmentRequest">The enrollment request to update</param>
    /// <returns>The updated enrollment request</returns>
    Task<EnrollmentRequest> UpdateEnrollmentRequestAsync(EnrollmentRequest enrollmentRequest);
    
    /// <summary>
    /// Approves an enrollment request
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <param name="teacherId">The ID of the teacher approving the request</param>
    /// <returns>The approved enrollment request</returns>
    Task<EnrollmentRequest?> ApproveEnrollmentRequestAsync(int enrollmentRequestId, int teacherId);
    
    /// <summary>
    /// Rejects an enrollment request
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <param name="teacherId">The ID of the teacher rejecting the request</param>
    /// <param name="reason">The reason for rejection</param>
    /// <returns>The rejected enrollment request</returns>
    Task<EnrollmentRequest?> RejectEnrollmentRequestAsync(int enrollmentRequestId, int teacherId, string? reason = null);
    
    /// <summary>
    /// Deletes an enrollment request
    /// </summary>
    /// <param name="enrollmentRequestId">The ID of the enrollment request</param>
    /// <returns>True if the enrollment request was deleted, false otherwise</returns>
    Task<bool> DeleteEnrollmentRequestAsync(int enrollmentRequestId);
    
    /// <summary>
    /// Checks if a student has a pending enrollment request for a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="studentId">The ID of the student</param>
    /// <returns>True if the student has a pending enrollment request, false otherwise</returns>
    Task<bool> HasPendingEnrollmentRequestAsync(int courseId, int studentId);
}
