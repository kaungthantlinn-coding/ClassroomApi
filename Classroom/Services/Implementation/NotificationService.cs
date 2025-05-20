using Classroom.Dtos.Notification;
using Classroom.Hubs;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Classroom.Services.Implementation;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICourseRepository _courseRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ICourseRepository courseRepository,
        INotificationRepository notificationRepository,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _courseRepository = courseRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task SendSubmissionNotificationAsync(Submission submission, Assignment assignment, User student, Course course)
    {
        try
        {
            // Get all teachers for this course
            var courseMembers = await _courseRepository.GetCourseMembersAsync(course.CourseId);
            var teachers = courseMembers.Where(cm => cm.Role == "Teacher").ToList();

            if (teachers.Count == 0)
            {
                _logger.LogWarning($"No teachers found for course {course.CourseId} to notify about submission {submission.SubmissionId}");
                return;
            }

            // Create the notification
            var notification = new SubmissionNotificationDto
            {
                Type = NotificationType.SubmissionCreated.ToString(),
                Title = "New Assignment Submission",
                Message = $"{student.Name} has submitted an assignment: {assignment.Title}",
                CreatedAt = DateTime.UtcNow,
                SubmissionId = submission.SubmissionId,
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                CourseId = course.CourseId,
                CourseName = course.Name,
                StudentId = student.UserId,
                StudentName = student.Name,
                SubmittedAt = submission.SubmittedAt,
                Data = new Dictionary<string, object>
                {
                    { "submissionId", submission.SubmissionId },
                    { "assignmentId", assignment.AssignmentId },
                    { "assignmentTitle", assignment.Title },
                    { "courseId", course.CourseId },
                    { "courseName", course.Name },
                    { "studentId", student.UserId },
                    { "studentName", student.Name },
                    { "submittedAt", submission.SubmittedAt }
                }
            };

            // Store notifications in the database and send via SignalR
            foreach (var teacher in teachers)
            {
                // Create a database notification
                var dbNotification = new Notification
                {
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    IsRead = false,
                    UserId = teacher.UserId,
                    CourseId = course.CourseId,
                    AssignmentId = assignment.AssignmentId,
                    SubmissionId = submission.SubmissionId,
                    Data = notification.Data
                };

                // Save to database
                await _notificationRepository.CreateNotificationAsync(dbNotification);

                // Send via SignalR
                notification.NotificationId = dbNotification.NotificationId;
                await SendNotificationToUserAsync(teacher.UserId, notification);
            }

            // Also send to the course group
            await SendNotificationToCourseAsync(course.CourseId, notification);

            _logger.LogInformation($"Sent submission notification to {teachers.Count} teachers for course {course.CourseId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending submission notification for submission {submission.SubmissionId}");
        }
    }

    public async Task SendNotificationToUserAsync(int userId, NotificationDto notification)
    {
        try
        {
            // If the notification doesn't have an ID yet, store it in the database
            if (notification.NotificationId <= 0)
            {
                var dbNotification = new Notification
                {
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    IsRead = false,
                    UserId = userId,
                    Data = notification.Data
                };

                // Add optional properties if they exist in the notification
                if (notification is SubmissionNotificationDto submissionNotification)
                {
                    dbNotification.CourseId = submissionNotification.CourseId;
                    dbNotification.AssignmentId = submissionNotification.AssignmentId;
                    dbNotification.SubmissionId = submissionNotification.SubmissionId;
                }

                // Save to database
                var savedNotification = await _notificationRepository.CreateNotificationAsync(dbNotification);
                notification.NotificationId = savedNotification.NotificationId;
            }

            // Send via SignalR
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Sent notification to user {userId}: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to user {userId}");
        }
    }

    public async Task SendNotificationToRoleAsync(string role, NotificationDto notification)
    {
        try
        {
            // For role-based notifications, we don't store them in the database
            // because we don't know which specific users should receive them
            // Instead, we just broadcast them via SignalR
            await _hubContext.Clients.Group($"Role_{role}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Sent notification to role {role}: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to role {role}");
        }
    }

    public async Task SendNotificationToCourseAsync(int courseId, NotificationDto notification)
    {
        try
        {
            // For course-based notifications, we don't store them in the database
            // because we don't know which specific users should receive them
            // Instead, we just broadcast them via SignalR
            await _hubContext.Clients.Group($"Course_{courseId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Sent notification to course {courseId}: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to course {courseId}");
        }
    }

    public async Task<List<NotificationDto>> GetNotificationsForUserAsync(int userId, int? limit = null, int? offset = null)
    {
        try
        {
            var notifications = await _notificationRepository.GetNotificationsForUserAsync(userId, limit, offset);
            return notifications.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notifications for user {userId}");
            throw;
        }
    }

    public async Task<List<NotificationDto>> GetUnreadNotificationsForUserAsync(int userId)
    {
        try
        {
            var notifications = await _notificationRepository.GetUnreadNotificationsForUserAsync(userId);
            return notifications.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notifications for user {userId}");
            throw;
        }
    }

    public async Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
            {
                return null;
            }
            return MapToDto(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notification {notificationId} for user {userId}");
            throw;
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
    {
        try
        {
            return await _notificationRepository.MarkNotificationAsReadAsync(notificationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking notification {notificationId} as read for user {userId}");
            throw;
        }
    }

    public async Task<int> MarkAllNotificationsAsReadAsync(int userId)
    {
        try
        {
            return await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking all notifications as read for user {userId}");
            throw;
        }
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
    {
        try
        {
            return await _notificationRepository.DeleteNotificationAsync(notificationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting notification {notificationId} for user {userId}");
            throw;
        }
    }

    public async Task<int> GetUnreadNotificationCountAsync(int userId)
    {
        try
        {
            return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notification count for user {userId}");
            throw;
        }
    }

    // Helper method to map from Notification entity to NotificationDto
    private NotificationDto MapToDto(Notification notification)
    {
        var dto = new NotificationDto
        {
            NotificationId = notification.NotificationId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt,
            IsRead = notification.IsRead,
            Data = notification.Data
        };

        return dto;
    }
}
