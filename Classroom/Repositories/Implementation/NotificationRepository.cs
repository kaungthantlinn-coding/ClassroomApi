using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class NotificationRepository : INotificationRepository
{
    private readonly ClassroomContext _context;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(ClassroomContext context, ILogger<NotificationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        try
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating notification for user {notification.UserId}");
            throw;
        }
    }

    public async Task<List<Notification>> GetNotificationsForUserAsync(int userId, int? limit = null, int? offset = null)
    {
        try
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notifications for user {userId}");
            throw;
        }
    }

    public async Task<List<Notification>> GetUnreadNotificationsForUserAsync(int userId)
    {
        try
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notifications for user {userId}");
            throw;
        }
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
    {
        try
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notification with ID {notificationId}");
            throw;
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
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
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return unreadNotifications.Count;
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
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
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
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notification count for user {userId}");
            throw;
        }
    }
}
