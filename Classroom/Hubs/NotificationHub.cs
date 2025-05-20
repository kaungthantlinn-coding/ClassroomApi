using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Classroom.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to a group based on their ID for individual notifications
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                
                // Add user to a group based on their role for role-based notifications
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }

                _logger.LogInformation($"User {userId} with role {userRole} connected with connection ID {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove user from their individual group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                
                // Remove user from their role group
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }

                _logger.LogInformation($"User {userId} with role {userRole} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method to join a course-specific group
        public async Task JoinCourseGroup(int courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Course_{courseId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined Course_{courseId} group");
        }

        // Method to leave a course-specific group
        public async Task LeaveCourseGroup(int courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Course_{courseId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} left Course_{courseId} group");
        }
    }
}
