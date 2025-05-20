using Classroom.Dtos.Notification;
using Classroom.Models;

namespace Classroom.Services.Interface;

public interface INotificationService
{
    /// <summary>
    /// Sends a notification about a new submission to all teachers of a course
    /// </summary>
    /// <param name="submission">The submission that was created</param>
    /// <param name="assignment">The assignment the submission is for</param>
    /// <param name="student">The student who submitted</param>
    /// <param name="course">The course the assignment belongs to</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendSubmissionNotificationAsync(Submission submission, Assignment assignment, User student, Course course);

    /// <summary>
    /// Sends a notification to a specific user
    /// </summary>
    /// <param name="userId">The ID of the user to notify</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendNotificationToUserAsync(int userId, NotificationDto notification);

    /// <summary>
    /// Sends a notification to all users with a specific role
    /// </summary>
    /// <param name="role">The role to target (e.g., "Teacher", "Student")</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendNotificationToRoleAsync(string role, NotificationDto notification);

    /// <summary>
    /// Sends a notification to all users in a course
    /// </summary>
    /// <param name="courseId">The ID of the course</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendNotificationToCourseAsync(int courseId, NotificationDto notification);

    /// <summary>
    /// Gets all notifications for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Optional: The maximum number of notifications to return</param>
    /// <param name="offset">Optional: The number of notifications to skip</param>
    /// <returns>A list of notifications</returns>
    Task<List<NotificationDto>> GetNotificationsForUserAsync(int userId, int? limit = null, int? offset = null);

    /// <summary>
    /// Gets all unread notifications for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A list of unread notifications</returns>
    Task<List<NotificationDto>> GetUnreadNotificationsForUserAsync(int userId);

    /// <summary>
    /// Gets a notification by ID
    /// </summary>
    /// <param name="notificationId">The ID of the notification</param>
    /// <param name="userId">The ID of the user who owns the notification</param>
    /// <returns>The notification if found, null otherwise</returns>
    Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId);

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    /// <param name="notificationId">The ID of the notification</param>
    /// <param name="userId">The ID of the user who owns the notification</param>
    /// <returns>True if the notification was marked as read, false otherwise</returns>
    Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);

    /// <summary>
    /// Marks all notifications for a user as read
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The number of notifications marked as read</returns>
    Task<int> MarkAllNotificationsAsReadAsync(int userId);

    /// <summary>
    /// Deletes a notification
    /// </summary>
    /// <param name="notificationId">The ID of the notification</param>
    /// <param name="userId">The ID of the user who owns the notification</param>
    /// <returns>True if the notification was deleted, false otherwise</returns>
    Task<bool> DeleteNotificationAsync(int notificationId, int userId);

    /// <summary>
    /// Gets the count of unread notifications for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The count of unread notifications</returns>
    Task<int> GetUnreadNotificationCountAsync(int userId);
}
