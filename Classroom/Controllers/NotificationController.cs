using Classroom.Dtos.Notification;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

/// <summary>
/// Controller for managing notifications
/// </summary>
[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all notifications for the current user
    /// </summary>
    /// <param name="limit">Optional: The maximum number of notifications to return</param>
    /// <param name="offset">Optional: The number of notifications to skip</param>
    /// <returns>A list of notifications</returns>
    /// <response code="200">Returns the list of notifications</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int? limit = null, [FromQuery] int? offset = null)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var notifications = await _notificationService.GetNotificationsForUserAsync(currentUserId, limit, offset);
            return Ok(new { success = true, notifications });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notifications for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error getting notifications: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets all unread notifications for the current user
    /// </summary>
    /// <returns>A list of unread notifications</returns>
    /// <response code="200">Returns the list of unread notifications</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var notifications = await _notificationService.GetUnreadNotificationsForUserAsync(currentUserId);
            return Ok(new { success = true, notifications });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notifications for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error getting unread notifications: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets the count of unread notifications for the current user
    /// </summary>
    /// <returns>The count of unread notifications</returns>
    /// <response code="200">Returns the count of unread notifications</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadNotificationCount()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var count = await _notificationService.GetUnreadNotificationCountAsync(currentUserId);
            return Ok(new { success = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notification count for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error getting unread notification count: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets a notification by ID
    /// </summary>
    /// <param name="id">The ID of the notification</param>
    /// <returns>The notification if found</returns>
    /// <response code="200">Returns the notification if found</response>
    /// <response code="404">If the notification is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id, currentUserId);
            if (notification == null)
            {
                return NotFound(new { success = false, message = $"Notification with ID {id} not found" });
            }
            return Ok(new { success = true, notification });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notification {id} for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error getting notification: {ex.Message}" });
        }
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    /// <param name="id">The ID of the notification</param>
    /// <returns>Success message if marked as read</returns>
    /// <response code="200">Returns success message if marked as read</response>
    /// <response code="404">If the notification is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _notificationService.MarkNotificationAsReadAsync(id, currentUserId);
            if (!result)
            {
                return NotFound(new { success = false, message = $"Notification with ID {id} not found" });
            }
            return Ok(new { success = true, message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking notification {id} as read for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error marking notification as read: {ex.Message}" });
        }
    }

    /// <summary>
    /// Marks all notifications for the current user as read
    /// </summary>
    /// <returns>Success message with the number of notifications marked as read</returns>
    /// <response code="200">Returns success message with the number of notifications marked as read</response>
    /// <response code="500">If there's a server error</response>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var count = await _notificationService.MarkAllNotificationsAsReadAsync(currentUserId);
            return Ok(new { success = true, message = $"{count} notifications marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking all notifications as read for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error marking all notifications as read: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes a notification
    /// </summary>
    /// <param name="id">The ID of the notification</param>
    /// <returns>Success message if deleted</returns>
    /// <response code="200">Returns success message if deleted</response>
    /// <response code="404">If the notification is not found</response>
    /// <response code="500">If there's a server error</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _notificationService.DeleteNotificationAsync(id, currentUserId);
            if (!result)
            {
                return NotFound(new { success = false, message = $"Notification with ID {id} not found" });
            }
            return Ok(new { success = true, message = "Notification deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting notification {id} for user {currentUserId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error deleting notification: {ex.Message}" });
        }
    }
}
