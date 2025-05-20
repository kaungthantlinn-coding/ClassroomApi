using Classroom.Dtos.Enrollment;
using Classroom.Dtos.Notification;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Classroom.Services.Implementation;

public class EnrollmentRequestService : IEnrollmentRequestService
{
    private readonly IEnrollmentRequestRepository _enrollmentRequestRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly INotificationService _notificationService;
    private readonly ClassroomContext _context;
    private readonly ILogger<EnrollmentRequestService> _logger;

    public EnrollmentRequestService(
        IEnrollmentRequestRepository enrollmentRequestRepository,
        ICourseRepository courseRepository,
        INotificationService notificationService,
        ClassroomContext context,
        ILogger<EnrollmentRequestService> logger)
    {
        _enrollmentRequestRepository = enrollmentRequestRepository;
        _courseRepository = courseRepository;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentRequestDto> CreateEnrollmentRequestAsync(CreateEnrollmentRequestDto createEnrollmentRequestDto, int studentId)
    {
        // Check if the course exists
        var course = await _courseRepository.GetByIdAsync(createEnrollmentRequestDto.CourseId);
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with ID {createEnrollmentRequestDto.CourseId} not found");
        }

        // Check if the student is already enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(createEnrollmentRequestDto.CourseId, studentId);
        if (isEnrolled)
        {
            throw new InvalidOperationException("You are already enrolled in this course");
        }

        // Check if the student already has a pending enrollment request for this course
        var hasPendingRequest = await _enrollmentRequestRepository.HasPendingEnrollmentRequestAsync(createEnrollmentRequestDto.CourseId, studentId);
        if (hasPendingRequest)
        {
            throw new InvalidOperationException("You already have a pending enrollment request for this course");
        }

        // Get student details
        var student = await _context.Users.FindAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found");
        }

        // Create the enrollment request
        var enrollmentRequest = new EnrollmentRequest
        {
            CourseId = createEnrollmentRequestDto.CourseId,
            StudentId = studentId,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        var createdRequest = await _enrollmentRequestRepository.CreateEnrollmentRequestAsync(enrollmentRequest);

        // Send notification to course teachers
        await SendEnrollmentRequestNotificationAsync(createdRequest, course, student);

        // Map to DTO
        return MapToDto(createdRequest, course, student);
    }

    public async Task<EnrollmentRequestDto?> GetEnrollmentRequestByIdAsync(int enrollmentRequestId, int userId)
    {
        var enrollmentRequest = await _enrollmentRequestRepository.GetEnrollmentRequestByIdAsync(enrollmentRequestId);
        if (enrollmentRequest == null)
        {
            return null;
        }

        // Check if the user has access to this enrollment request
        // Teachers of the course or the student who made the request can access it
        bool isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(enrollmentRequest.CourseId, userId);
        bool isStudent = enrollmentRequest.StudentId == userId;

        if (!isTeacher && !isStudent)
        {
            throw new UnauthorizedAccessException("You don't have permission to access this enrollment request");
        }

        // Map to DTO
        return MapToDto(enrollmentRequest);
    }

    public async Task<List<EnrollmentRequestDto>> GetEnrollmentRequestsByCourseIdAsync(int courseId, int teacherId)
    {
        // Verify the teacher has access to this course
        bool isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You don't have permission to access enrollment requests for this course");
        }

        var enrollmentRequests = await _enrollmentRequestRepository.GetEnrollmentRequestsByCourseIdAsync(courseId);
        return enrollmentRequests.Select(er => MapToDto(er)).ToList();
    }

    public async Task<List<EnrollmentRequestDto>> GetEnrollmentRequestsByStudentIdAsync(int studentId)
    {
        var enrollmentRequests = await _enrollmentRequestRepository.GetEnrollmentRequestsByStudentIdAsync(studentId);
        return enrollmentRequests.Select(er => MapToDto(er)).ToList();
    }

    public async Task<List<EnrollmentRequestDto>> GetPendingEnrollmentRequestsByCourseIdAsync(int courseId, int teacherId)
    {
        // Verify the teacher has access to this course
        bool isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You don't have permission to access enrollment requests for this course");
        }

        var enrollmentRequests = await _enrollmentRequestRepository.GetPendingEnrollmentRequestsByCourseIdAsync(courseId);
        return enrollmentRequests.Select(er => MapToDto(er)).ToList();
    }

    public async Task<EnrollmentRequestDto?> ProcessEnrollmentRequestAsync(int enrollmentRequestId, ProcessEnrollmentRequestDto processEnrollmentRequestDto, int teacherId)
    {
        var enrollmentRequest = await _enrollmentRequestRepository.GetEnrollmentRequestByIdAsync(enrollmentRequestId);
        if (enrollmentRequest == null)
        {
            return null;
        }

        // Verify the teacher has access to this course
        bool isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(enrollmentRequest.CourseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You don't have permission to process enrollment requests for this course");
        }

        // Process the request
        if (processEnrollmentRequestDto.Action.ToLower() == "approve")
        {
            // Approve the request
            var approvedRequest = await _enrollmentRequestRepository.ApproveEnrollmentRequestAsync(enrollmentRequestId, teacherId);
            if (approvedRequest == null)
            {
                return null;
            }

            // Enroll the student in the course
            var courseMember = new CourseMember
            {
                CourseId = approvedRequest.CourseId,
                UserId = approvedRequest.StudentId,
                Role = "Student"
            };

            await _courseRepository.AddMemberAsync(courseMember);

            // Send notification to the student
            await SendEnrollmentApprovedNotificationAsync(approvedRequest);

            return MapToDto(approvedRequest);
        }
        else if (processEnrollmentRequestDto.Action.ToLower() == "reject")
        {
            // Reject the request
            var rejectedRequest = await _enrollmentRequestRepository.RejectEnrollmentRequestAsync(
                enrollmentRequestId,
                teacherId,
                processEnrollmentRequestDto.RejectionReason);

            if (rejectedRequest == null)
            {
                return null;
            }

            // Send notification to the student
            await SendEnrollmentRejectedNotificationAsync(rejectedRequest);

            return MapToDto(rejectedRequest);
        }
        else
        {
            throw new ArgumentException("Invalid action. Action must be 'approve' or 'reject'.");
        }
    }

    public async Task<bool> CancelEnrollmentRequestAsync(int enrollmentRequestId, int studentId)
    {
        var enrollmentRequest = await _enrollmentRequestRepository.GetEnrollmentRequestByIdAsync(enrollmentRequestId);
        if (enrollmentRequest == null)
        {
            return false;
        }

        // Verify the student is the owner of the request
        if (enrollmentRequest.StudentId != studentId)
        {
            throw new UnauthorizedAccessException("You don't have permission to cancel this enrollment request");
        }

        // Verify the request is still pending
        if (enrollmentRequest.Status != "Pending")
        {
            throw new InvalidOperationException("Cannot cancel an enrollment request that has already been processed");
        }

        return await _enrollmentRequestRepository.DeleteEnrollmentRequestAsync(enrollmentRequestId);
    }

    public async Task<bool> HasPendingEnrollmentRequestAsync(int courseId, int studentId)
    {
        return await _enrollmentRequestRepository.HasPendingEnrollmentRequestAsync(courseId, studentId);
    }

    // Helper methods
    private EnrollmentRequestDto MapToDto(EnrollmentRequest enrollmentRequest, Course? course = null, User? student = null)
    {
        course ??= enrollmentRequest.Course;
        student ??= enrollmentRequest.Student;

        return new EnrollmentRequestDto
        {
            EnrollmentRequestId = enrollmentRequest.EnrollmentRequestId,
            CourseId = enrollmentRequest.CourseId,
            CourseName = course?.Name ?? string.Empty,
            StudentId = enrollmentRequest.StudentId,
            StudentName = student?.Name ?? string.Empty,
            StudentEmail = student?.Email ?? string.Empty,
            RequestedAt = enrollmentRequest.RequestedAt,
            Status = enrollmentRequest.Status,
            ProcessedAt = enrollmentRequest.ProcessedAt,
            ProcessedById = enrollmentRequest.ProcessedById,
            ProcessedByName = enrollmentRequest.ProcessedBy?.Name,
            RejectionReason = enrollmentRequest.RejectionReason
        };
    }

    private async Task SendEnrollmentRequestNotificationAsync(EnrollmentRequest enrollmentRequest, Course course, User student)
    {
        try
        {
            // Get all teachers for the course
            var courseMembers = await _courseRepository.GetCourseMembersAsync(course.CourseId);
            var teachers = courseMembers.Where(cm => cm.Role == "Teacher").ToList();

            // Create notification
            var notification = new EnrollmentRequestNotificationDto
            {
                Type = NotificationType.EnrollmentRequest.ToString(),
                Title = "New Enrollment Request",
                Message = $"{student.Name} has requested to join your course '{course.Name}'",
                CreatedAt = DateTime.UtcNow,
                CourseId = course.CourseId,
                CourseName = course.Name,
                StudentId = student.UserId,
                StudentName = student.Name,
                StudentEmail = student.Email,
                RequestedAt = enrollmentRequest.RequestedAt,
                EnrollmentStatus = "Pending",
                Data = new Dictionary<string, object>
                {
                    { "enrollmentRequestId", enrollmentRequest.EnrollmentRequestId },
                    { "courseId", course.CourseId },
                    { "studentId", student.UserId }
                }
            };

            // Send notification to all teachers
            foreach (var teacher in teachers)
            {
                await _notificationService.SendNotificationToUserAsync(teacher.UserId, notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending enrollment request notification for student {student.UserId} and course {course.CourseId}");
        }
    }

    private async Task SendEnrollmentApprovedNotificationAsync(EnrollmentRequest enrollmentRequest)
    {
        try
        {
            // Get course and student details
            var course = await _context.Courses.FindAsync(enrollmentRequest.CourseId);
            var student = await _context.Users.FindAsync(enrollmentRequest.StudentId);
            var teacher = await _context.Users.FindAsync(enrollmentRequest.ProcessedById);

            if (course == null || student == null)
            {
                return;
            }

            // Create notification
            var notification = new NotificationDto
            {
                Type = NotificationType.EnrollmentApproved.ToString(),
                Title = "Enrollment Request Approved",
                Message = $"Your request to join the course '{course.Name}' has been approved",
                CreatedAt = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    { "enrollmentRequestId", enrollmentRequest.EnrollmentRequestId },
                    { "courseId", course.CourseId },
                    { "courseName", course.Name },
                    { "processedBy", teacher?.Name ?? "A teacher" },
                    { "processedAt", enrollmentRequest.ProcessedAt ?? DateTime.UtcNow }
                }
            };

            // Send notification to the student
            await _notificationService.SendNotificationToUserAsync(student.UserId, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending enrollment approved notification for request {enrollmentRequest.EnrollmentRequestId}");
        }
    }

    private async Task SendEnrollmentRejectedNotificationAsync(EnrollmentRequest enrollmentRequest)
    {
        try
        {
            // Get course and student details
            var course = await _context.Courses.FindAsync(enrollmentRequest.CourseId);
            var student = await _context.Users.FindAsync(enrollmentRequest.StudentId);
            var teacher = await _context.Users.FindAsync(enrollmentRequest.ProcessedById);

            if (course == null || student == null)
            {
                return;
            }

            // Create notification
            var notification = new NotificationDto
            {
                Type = NotificationType.EnrollmentRejected.ToString(),
                Title = "Enrollment Request Rejected",
                Message = $"Your request to join the course '{course.Name}' has been rejected",
                CreatedAt = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    { "enrollmentRequestId", enrollmentRequest.EnrollmentRequestId },
                    { "courseId", course.CourseId },
                    { "courseName", course.Name },
                    { "processedBy", teacher?.Name ?? "A teacher" },
                    { "processedAt", enrollmentRequest.ProcessedAt ?? DateTime.UtcNow },
                    { "rejectionReason", enrollmentRequest.RejectionReason ?? "No reason provided" }
                }
            };

            // Send notification to the student
            await _notificationService.SendNotificationToUserAsync(student.UserId, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending enrollment rejected notification for request {enrollmentRequest.EnrollmentRequestId}");
        }
    }
}
