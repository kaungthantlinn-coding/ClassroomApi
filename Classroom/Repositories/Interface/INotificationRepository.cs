using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface INotificationRepository
{
    /// <summary>
    /// Creates a new notification
    /// </summary>
    /// <param name="notification">The notification to create</param>
    /// <returns>The created notification</returns>
    Task<Notification> CreateNotificationAsync(Notification notification);
    
    /// <summary>
    /// Gets all notifications for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Optional: The maximum number of notifications to return</param>
    /// <param name="offset">Optional: The number of notifications to skip</param>
    /// <returns>A list of notifications</returns>
    Task<List<Notification>> GetNotificationsForUserAsync(int userId, int? limit = null, int? offset = null);
    
    /// <summary>
    /// Gets all unread notifications for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A list of unread notifications</returns>
    Task<List<Notification>> GetUnreadNotificationsForUserAsync(int userId);
    
    /// <summary>
    /// Gets a notification by ID
    /// </summary>
    /// <param name="notificationId">The ID of the notification</param>
    /// <returns>The notification if found, null otherwise</returns>
    Task<Notification?> GetNotificationByIdAsync(int notificationId);
    
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
