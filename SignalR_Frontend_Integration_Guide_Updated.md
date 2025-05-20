# SignalR Notification System - Frontend Integration Guide

This guide explains how to integrate the SignalR real-time notification system with your React frontend to display instant alerts when students submit assignments. It includes both the SignalR real-time notifications and the REST API endpoints for retrieving and managing notifications.

## Prerequisites

1. Install the required npm packages:

```bash
npm install @microsoft/signalr axios date-fns
```

2. Make sure your backend has the following components set up:
   - Notification model and database table
   - NotificationHub for SignalR
   - NotificationController with REST endpoints
   - NotificationService for sending and managing notifications

## Step 1: Create a Notification API Client

Create a new file `src/api/notificationApi.ts`:

```typescript
import axios from "axios";
import { getAuthHeader } from "./authUtils"; // Adjust based on your auth implementation

const API_URL = process.env.REACT_APP_API_URL || "http://localhost:5000/api";

// Define the notification interface
export interface Notification {
  notificationId: number;
  type: string;
  title: string;
  message: string;
  createdAt: string;
  isRead: boolean;
  data: Record<string, any>;
}

// Create an axios instance with default config
const notificationApi = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add auth header to every request
notificationApi.interceptors.request.use(
  async (config) => {
    const authHeader = await getAuthHeader();
    if (authHeader) {
      config.headers.Authorization = authHeader;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Get all notifications with pagination
export const getNotifications = async (limit = 20, offset = 0) => {
  try {
    const response = await notificationApi.get(
      `/notifications?limit=${limit}&offset=${offset}`
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching notifications:", error);
    throw error;
  }
};

// Get unread notifications
export const getUnreadNotifications = async () => {
  try {
    const response = await notificationApi.get("/notifications/unread");
    return response.data;
  } catch (error) {
    console.error("Error fetching unread notifications:", error);
    throw error;
  }
};

// Get unread notification count
export const getUnreadNotificationCount = async () => {
  try {
    const response = await notificationApi.get("/notifications/unread/count");
    return response.data.count;
  } catch (error) {
    console.error("Error fetching unread notification count:", error);
    return 0; // Return 0 as a fallback
  }
};

// Mark a notification as read
export const markNotificationAsRead = async (notificationId: number) => {
  try {
    const response = await notificationApi.put(
      `/notifications/${notificationId}/read`
    );
    return response.data;
  } catch (error) {
    console.error("Error marking notification as read:", error);
    throw error;
  }
};

// Mark all notifications as read
export const markAllNotificationsAsRead = async () => {
  try {
    const response = await notificationApi.put("/notifications/read-all");
    return response.data;
  } catch (error) {
    console.error("Error marking all notifications as read:", error);
    throw error;
  }
};

// Delete a notification
export const deleteNotification = async (notificationId: number) => {
  try {
    const response = await notificationApi.delete(
      `/notifications/${notificationId}`
    );
    return response.data;
  } catch (error) {
    console.error("Error deleting notification:", error);
    throw error;
  }
};

export default notificationApi;
```

## Step 2: Create a SignalR Service

Create a new file `src/services/signalRService.ts`:

```typescript
import * as signalR from "@microsoft/signalr";
import { getToken } from "./authService"; // Adjust based on your auth implementation
import { Notification } from "../api/notificationApi";

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationCallbacks: ((notification: Notification) => void)[] = [];
  private connectionStarted: boolean = false;
  private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 5;

  // Initialize the connection to the SignalR hub
  public async startConnection(): Promise<boolean> {
    if (this.connectionStarted) {
      console.log("SignalR connection already started");
      return true;
    }

    try {
      const token = await getToken();

      // Build the connection with custom retry policy
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(
          `${process.env.REACT_APP_API_URL}/hubs/notifications?access_token=${token}`
        )
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
              // Stop trying after max attempts
              return null;
            }

            // Exponential backoff: 0.5s, 1s, 2s, 4s, 8s
            return Math.min(
              1000 * Math.pow(2, retryContext.previousRetryCount),
              30000
            );
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up connection event handlers
      this.hubConnection.onreconnecting((error) => {
        console.warn(
          "SignalR connection lost. Attempting to reconnect...",
          error
        );
        this.reconnectAttempts++;
      });

      this.hubConnection.onreconnected((connectionId) => {
        console.log(
          "SignalR connection reestablished. ConnectionId:",
          connectionId
        );
        this.reconnectAttempts = 0;
      });

      this.hubConnection.onclose((error) => {
        console.error("SignalR connection closed", error);
        this.connectionStarted = false;
      });

      // Set up notification event handler
      this.hubConnection.on(
        "ReceiveNotification",
        (notification: Notification) => {
          console.log("Received notification:", notification);
          this.notificationCallbacks.forEach((callback) =>
            callback(notification)
          );
        }
      );

      // Start the connection
      await this.hubConnection.start();
      console.log("SignalR connection established successfully");
      this.connectionStarted = true;
      this.reconnectAttempts = 0;
      return true;
    } catch (error) {
      console.error("Error establishing SignalR connection:", error);
      this.connectionStarted = false;
      return false;
    }
  }

  // Stop the connection when no longer needed
  public async stopConnection(): Promise<boolean> {
    if (!this.hubConnection) {
      return true;
    }

    try {
      await this.hubConnection.stop();
      console.log("SignalR connection stopped");
      this.connectionStarted = false;
      return true;
    } catch (error) {
      console.error("Error stopping SignalR connection:", error);
      return false;
    }
  }

  // Join a course-specific group to receive notifications for that course
  public async joinCourseGroup(courseId: number): Promise<boolean> {
    if (!this.ensureConnection()) {
      return false;
    }

    try {
      await this.hubConnection!.invoke("JoinCourseGroup", courseId);
      console.log(`Joined course group ${courseId}`);
      return true;
    } catch (error) {
      console.error(`Error joining course group ${courseId}:`, error);
      return false;
    }
  }

  // Leave a course-specific group
  public async leaveCourseGroup(courseId: number): Promise<boolean> {
    if (!this.ensureConnection()) {
      return false;
    }

    try {
      await this.hubConnection!.invoke("LeaveCourseGroup", courseId);
      console.log(`Left course group ${courseId}`);
      return true;
    } catch (error) {
      console.error(`Error leaving course group ${courseId}:`, error);
      return false;
    }
  }

  // Register a callback to be called when a notification is received
  public onNotification(callback: (notification: Notification) => void): void {
    this.notificationCallbacks.push(callback);
  }

  // Remove a callback
  public offNotification(callback: (notification: Notification) => void): void {
    this.notificationCallbacks = this.notificationCallbacks.filter(
      (cb) => cb !== callback
    );
  }

  // Helper method to ensure connection is established
  private ensureConnection(): boolean {
    if (
      !this.hubConnection ||
      this.hubConnection.state !== signalR.HubConnectionState.Connected
    ) {
      console.warn(
        "SignalR connection not established. Attempting to reconnect..."
      );
      this.startConnection();
      return false;
    }
    return true;
  }

  // Get the current connection state
  public getConnectionState(): signalR.HubConnectionState | null {
    return this.hubConnection ? this.hubConnection.state : null;
  }

  // Check if the connection is active
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }
}

// Create a singleton instance
const signalRService = new SignalRService();
export default signalRService;
```

## Step 3: Create a Notification Context

Create a new file `src/contexts/NotificationContext.tsx`:

```typescript
import React, { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "./AuthContext"; // Adjust based on your auth implementation
import signalRService from "../services/signalRService";
import {
  Notification,
  getNotifications,
  getUnreadNotifications,
  getUnreadNotificationCount,
  markNotificationAsRead,
  markAllNotificationsAsRead,
} from "../api/notificationApi";

// Define the context type
interface NotificationContextType {
  notifications: Notification[];
  unreadCount: number;
  loading: boolean;
  markAsRead: (notificationId: number) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  joinCourseGroup: (courseId: number) => Promise<void>;
  leaveCourseGroup: (courseId: number) => Promise<void>;
}

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined
);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const { isAuthenticated } = useAuth(); // Adjust based on your auth context

  // Initialize notifications and SignalR connection
  useEffect(() => {
    if (isAuthenticated) {
      const initializeNotifications = async () => {
        try {
          setLoading(true);

          // Start SignalR connection
          await signalRService.startConnection();

          // Get initial notifications
          const response = await getNotifications();
          if (response.success) {
            setNotifications(response.notifications);
          }

          // Get unread count
          const count = await getUnreadNotificationCount();
          setUnreadCount(count);

          setLoading(false);
        } catch (error) {
          console.error("Error initializing notifications:", error);
          setLoading(false);
        }
      };

      initializeNotifications();

      // Set up SignalR notification handler
      const handleNotification = (notification: Notification) => {
        setNotifications((prev) => [notification, ...prev]);
        if (!notification.isRead) {
          setUnreadCount((prev) => prev + 1);
        }
      };

      signalRService.onNotification(handleNotification);

      // Clean up
      return () => {
        signalRService.offNotification(handleNotification);
        signalRService.stopConnection();
      };
    }
  }, [isAuthenticated]);

  // Mark a notification as read
  const markAsRead = async (notificationId: number) => {
    try {
      const response = await markNotificationAsRead(notificationId);
      if (response.success) {
        setNotifications((prev) =>
          prev.map((n) =>
            n.notificationId === notificationId ? { ...n, isRead: true } : n
          )
        );
        setUnreadCount((prev) => Math.max(0, prev - 1));
      }
    } catch (error) {
      console.error("Error marking notification as read:", error);
    }
  };

  // Mark all notifications as read
  const markAllAsRead = async () => {
    try {
      const response = await markAllNotificationsAsRead();
      if (response.success) {
        setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
        setUnreadCount(0);
      }
    } catch (error) {
      console.error("Error marking all notifications as read:", error);
    }
  };

  // Join a course group to receive notifications for that course
  const joinCourseGroup = async (courseId: number) => {
    if (isAuthenticated) {
      await signalRService.joinCourseGroup(courseId);
    }
  };

  // Leave a course group
  const leaveCourseGroup = async (courseId: number) => {
    if (isAuthenticated) {
      await signalRService.leaveCourseGroup(courseId);
    }
  };

  return (
    <NotificationContext.Provider
      value={{
        notifications,
        unreadCount,
        loading,
        markAsRead,
        markAllAsRead,
        joinCourseGroup,
        leaveCourseGroup,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};

// Custom hook to use the notification context
export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error(
      "useNotifications must be used within a NotificationProvider"
    );
  }
  return context;
};
```

## Step 4: Create a Notification Bell Component

Create a new file `src/components/NotificationBell.tsx`:

```typescript
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useNotifications } from "../contexts/NotificationContext";
import {
  Badge,
  IconButton,
  Menu,
  MenuItem,
  Typography,
  Box,
  Divider,
  CircularProgress,
} from "@mui/material";
import NotificationsIcon from "@mui/icons-material/Notifications";
import { formatDistanceToNow } from "date-fns";

const NotificationBell: React.FC = () => {
  const { notifications, unreadCount, loading, markAsRead, markAllAsRead } =
    useNotifications();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const navigate = useNavigate();

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNotificationClick = (notification: any) => {
    markAsRead(notification.notificationId);

    // Navigate based on notification type
    if (notification.type === "SubmissionCreated") {
      // Extract data from notification
      const { assignmentId, courseId, submissionId } = notification.data;

      // Navigate to the submission
      navigate(
        `/class/${courseId}/assignment/${assignmentId}/submission/${submissionId}`
      );
    }

    handleClose();
  };

  return (
    <>
      <IconButton color="inherit" onClick={handleOpen}>
        <Badge badgeContent={unreadCount} color="error">
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        PaperProps={{
          style: {
            maxHeight: 400,
            width: 350,
          },
        }}
      >
        <Box
          sx={{
            p: 1,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="h6">Notifications</Typography>
          {unreadCount > 0 && (
            <Typography
              variant="body2"
              color="primary"
              sx={{ cursor: "pointer" }}
              onClick={markAllAsRead}
            >
              Mark all as read
            </Typography>
          )}
        </Box>

        <Divider />

        {loading ? (
          <Box sx={{ display: "flex", justifyContent: "center", p: 2 }}>
            <CircularProgress size={24} />
          </Box>
        ) : notifications.length === 0 ? (
          <MenuItem disabled>
            <Typography variant="body2">No notifications</Typography>
          </MenuItem>
        ) : (
          notifications.map((notification) => (
            <MenuItem
              key={notification.notificationId}
              onClick={() => handleNotificationClick(notification)}
              sx={{
                backgroundColor: notification.isRead
                  ? "inherit"
                  : "rgba(0, 0, 0, 0.04)",
                borderLeft: notification.isRead ? "none" : "3px solid #1976d2",
              }}
            >
              <Box sx={{ width: "100%" }}>
                <Typography variant="subtitle2">
                  {notification.title}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {notification.message}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {formatDistanceToNow(new Date(notification.createdAt), {
                    addSuffix: true,
                  })}
                </Typography>
              </Box>
            </MenuItem>
          ))
        )}
      </Menu>
    </>
  );
};

export default NotificationBell;
```

## Step 5: Add the NotificationProvider to Your App

Update your `src/App.tsx` or main component:

```typescript
import React from "react";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import { NotificationProvider } from "./contexts/NotificationContext";
import AppRoutes from "./routes";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NotificationProvider>
          <AppRoutes />
        </NotificationProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
```

## Step 6: Add the NotificationBell to Your Layout

Add the NotificationBell component to your main layout or navbar:

```typescript
import React from "react";
import { AppBar, Toolbar, Typography, Box } from "@mui/material";
import NotificationBell from "./NotificationBell";

const Navbar: React.FC = () => {
  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Classroom
        </Typography>
        <Box>
          <NotificationBell />
          {/* Other navbar items */}
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;
```

## Step 7: Join Course Groups in Course Pages

When a teacher navigates to a specific course page, make sure to join the course-specific SignalR group:

```typescript
import React, { useEffect } from "react";
import { useParams } from "react-router-dom";
import { useNotifications } from "../contexts/NotificationContext";

const CoursePage: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const { joinCourseGroup, leaveCourseGroup } = useNotifications();

  useEffect(() => {
    if (courseId) {
      // Join the course group to receive notifications for this course
      joinCourseGroup(parseInt(courseId));

      // Clean up when leaving the page
      return () => {
        leaveCourseGroup(parseInt(courseId));
      };
    }
  }, [courseId, joinCourseGroup, leaveCourseGroup]);

  // Rest of your component...
};
```

## Troubleshooting

### 404 Error for /api/notifications

If you're seeing a 404 error when trying to access the `/api/notifications` endpoint, check the following:

1. Make sure the `NotificationController` is properly registered in your backend
2. Verify that the route is correctly defined as `[Route("api/notifications")]` in the controller
3. Check that the database table for notifications has been created
4. Ensure the application has been restarted after making changes

### SignalR Connection Issues

If you're having trouble with the SignalR connection:

1. Check browser console for any connection errors
2. Verify that the SignalR hub URL is correct (should be `/hubs/notifications`)
3. Ensure the JWT token is being passed correctly in the query string
4. Check that CORS is properly configured to allow SignalR connections
5. Verify that the `NotificationHub` is mapped in `Program.cs` with `app.MapHub<NotificationHub>("/hubs/notifications")`

### Manual Testing

You can manually test the notification system by sending a test notification:

```typescript
// In browser console
const testNotification = {
  notificationId: `notification-${Date.now()}`,
  type: "submission",
  title: "New Submission",
  message: "Student User submitted assignment 'Test'",
  createdAt: new Date().toISOString(),
  isRead: false,
  userId: "student-123",
  courseId: "3",
  assignmentId: "1033",
  link: "/class/3/assignment/1033",
  data: {},
};

// Broadcast a manual notification event
window.dispatchEvent(
  new CustomEvent("manual-notification", { detail: testNotification })
);
```

Then add this code to your `NotificationContext.tsx`:

```typescript
// Add manual notification handler for testing
useEffect(() => {
  const handleManualNotification = (event: any) => {
    console.log("Received manual notification event:", event.detail);
    handleNotification(event.detail);
  };

  window.addEventListener("manual-notification", handleManualNotification);

  return () => {
    window.removeEventListener("manual-notification", handleManualNotification);
  };
}, []);
```

## Conclusion

You've now implemented a complete real-time notification system for your Classroom application. This system combines:

1. **REST API endpoints** for storing, retrieving, and managing notifications
2. **SignalR real-time communication** for instant delivery of notifications
3. **React components** for displaying notifications to users

This implementation provides teachers with immediate alerts when students submit assignments, enhancing the responsiveness of your application and improving the user experience.

### Next Steps

To further enhance your notification system, consider:

1. **Notification preferences**: Allow users to customize which notifications they receive
2. **Email notifications**: Send email notifications for important events
3. **Mobile push notifications**: Implement push notifications for mobile devices
4. **Notification categories**: Organize notifications by type (submissions, announcements, etc.)
5. **Notification history**: Provide a dedicated page for viewing all notifications

Remember to thoroughly test your implementation across different browsers and devices to ensure a consistent experience for all users.
