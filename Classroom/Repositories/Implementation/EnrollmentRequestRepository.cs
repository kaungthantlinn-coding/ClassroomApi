using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class EnrollmentRequestRepository : IEnrollmentRequestRepository
{
    private readonly ClassroomContext _context;
    private readonly ILogger<EnrollmentRequestRepository> _logger;

    public EnrollmentRequestRepository(ClassroomContext context, ILogger<EnrollmentRequestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentRequest> CreateEnrollmentRequestAsync(EnrollmentRequest enrollmentRequest)
    {
        try
        {
            _context.EnrollmentRequests.Add(enrollmentRequest);
            await _context.SaveChangesAsync();
            return enrollmentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating enrollment request for student {enrollmentRequest.StudentId} and course {enrollmentRequest.CourseId}");
            throw;
        }
    }

    public async Task<EnrollmentRequest?> GetEnrollmentRequestByIdAsync(int enrollmentRequestId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .Include(er => er.Course)
                .Include(er => er.Student)
                .Include(er => er.ProcessedBy)
                .FirstOrDefaultAsync(er => er.EnrollmentRequestId == enrollmentRequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment request with ID {enrollmentRequestId}");
            throw;
        }
    }

    public async Task<List<EnrollmentRequest>> GetEnrollmentRequestsByCourseIdAsync(int courseId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .Include(er => er.Student)
                .Include(er => er.ProcessedBy)
                .Where(er => er.CourseId == courseId)
                .OrderByDescending(er => er.RequestedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment requests for course {courseId}");
            throw;
        }
    }

    public async Task<List<EnrollmentRequest>> GetEnrollmentRequestsByStudentIdAsync(int studentId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .Include(er => er.Course)
                .Include(er => er.ProcessedBy)
                .Where(er => er.StudentId == studentId)
                .OrderByDescending(er => er.RequestedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment requests for student {studentId}");
            throw;
        }
    }

    public async Task<List<EnrollmentRequest>> GetPendingEnrollmentRequestsByCourseIdAsync(int courseId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .Include(er => er.Student)
                .Where(er => er.CourseId == courseId && er.Status == "Pending")
                .OrderByDescending(er => er.RequestedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting pending enrollment requests for course {courseId}");
            throw;
        }
    }

    public async Task<EnrollmentRequest?> GetEnrollmentRequestByCourseAndStudentAsync(int courseId, int studentId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .Include(er => er.Course)
                .Include(er => er.Student)
                .Include(er => er.ProcessedBy)
                .FirstOrDefaultAsync(er => er.CourseId == courseId && er.StudentId == studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting enrollment request for student {studentId} and course {courseId}");
            throw;
        }
    }

    public async Task<EnrollmentRequest> UpdateEnrollmentRequestAsync(EnrollmentRequest enrollmentRequest)
    {
        try
        {
            _context.EnrollmentRequests.Update(enrollmentRequest);
            await _context.SaveChangesAsync();
            return enrollmentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating enrollment request with ID {enrollmentRequest.EnrollmentRequestId}");
            throw;
        }
    }

    public async Task<EnrollmentRequest?> ApproveEnrollmentRequestAsync(int enrollmentRequestId, int teacherId)
    {
        try
        {
            var enrollmentRequest = await GetEnrollmentRequestByIdAsync(enrollmentRequestId);
            if (enrollmentRequest == null)
            {
                return null;
            }

            enrollmentRequest.Status = "Approved";
            enrollmentRequest.ProcessedAt = DateTime.UtcNow;
            enrollmentRequest.ProcessedById = teacherId;

            await _context.SaveChangesAsync();
            return enrollmentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving enrollment request with ID {enrollmentRequestId}");
            throw;
        }
    }

    public async Task<EnrollmentRequest?> RejectEnrollmentRequestAsync(int enrollmentRequestId, int teacherId, string? reason = null)
    {
        try
        {
            var enrollmentRequest = await GetEnrollmentRequestByIdAsync(enrollmentRequestId);
            if (enrollmentRequest == null)
            {
                return null;
            }

            enrollmentRequest.Status = "Rejected";
            enrollmentRequest.ProcessedAt = DateTime.UtcNow;
            enrollmentRequest.ProcessedById = teacherId;
            enrollmentRequest.RejectionReason = reason;

            await _context.SaveChangesAsync();
            return enrollmentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting enrollment request with ID {enrollmentRequestId}");
            throw;
        }
    }

    public async Task<bool> DeleteEnrollmentRequestAsync(int enrollmentRequestId)
    {
        try
        {
            var enrollmentRequest = await _context.EnrollmentRequests.FindAsync(enrollmentRequestId);
            if (enrollmentRequest == null)
            {
                return false;
            }

            _context.EnrollmentRequests.Remove(enrollmentRequest);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting enrollment request with ID {enrollmentRequestId}");
            throw;
        }
    }

    public async Task<bool> HasPendingEnrollmentRequestAsync(int courseId, int studentId)
    {
        try
        {
            return await _context.EnrollmentRequests
                .AnyAsync(er => er.CourseId == courseId && er.StudentId == studentId && er.Status == "Pending");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if student {studentId} has pending enrollment request for course {courseId}");
            throw;
        }
    }
}
