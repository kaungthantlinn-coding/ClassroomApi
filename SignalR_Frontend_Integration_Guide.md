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

## Step 1: Create a SignalR Service

Create a new file `src/services/signalRService.ts`:

```typescript
import * as signalR from "@microsoft/signalr";
import { getToken } from "./authService"; // Adjust based on your auth implementation

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationCallbacks: ((notification: any) => void)[] = [];

  // Initialize the connection to the SignalR hub
  public async startConnection(): Promise<void> {
    try {
      const token = await getToken();

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(
          `${process.env.REACT_APP_API_URL}/hubs/notifications?access_token=${token}`
        )
        .withAutomaticReconnect()
        .build();

      // Set up event handlers
      this.hubConnection.on("ReceiveNotification", (notification) => {
        console.log("Received notification:", notification);
        this.notificationCallbacks.forEach((callback) =>
          callback(notification)
        );
      });

      // Start the connection
      await this.hubConnection.start();
      console.log("SignalR connection established");

      // Join course groups for teachers
      // This should be called after navigating to a course page
      // Example: signalRService.joinCourseGroup(courseId);
    } catch (error) {
      console.error("Error establishing SignalR connection:", error);
    }
  }

  // Stop the connection when no longer needed
  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      console.log("SignalR connection stopped");
    }
  }

  // Join a course-specific group to receive notifications for that course
  public async joinCourseGroup(courseId: number): Promise<void> {
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      await this.hubConnection.invoke("JoinCourseGroup", courseId);
      console.log(`Joined course group ${courseId}`);
    }
  }

  // Leave a course-specific group
  public async leaveCourseGroup(courseId: number): Promise<void> {
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      await this.hubConnection.invoke("LeaveCourseGroup", courseId);
      console.log(`Left course group ${courseId}`);
    }
  }

  // Register a callback to be called when a notification is received
  public onNotification(callback: (notification: any) => void): void {
    this.notificationCallbacks.push(callback);
  }

  // Remove a callback
  public offNotification(callback: (notification: any) => void): void {
    this.notificationCallbacks = this.notificationCallbacks.filter(
      (cb) => cb !== callback
    );
  }
}

// Create a singleton instance
const signalRService = new SignalRService();
export default signalRService;
```

## Step 2: Create a Notification Context

Create a new file `src/contexts/NotificationContext.tsx`:

```typescript
import React, { createContext, useContext, useEffect, useState } from "react";
import signalRService from "../services/signalRService";
import { useAuth } from "./AuthContext"; // Adjust based on your auth context

// Define the notification type
export interface Notification {
  notificationId: number;
  type: string;
  title: string;
  message: string;
  createdAt: string;
  isRead: boolean;
  data: Record<string, any>;
}

// Define the context type
interface NotificationContextType {
  notifications: Notification[];
  unreadCount: number;
  markAsRead: (notificationId: number) => void;
  markAllAsRead: () => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined
);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const { isAuthenticated, user } = useAuth(); // Adjust based on your auth context

  useEffect(() => {
    if (isAuthenticated) {
      // Start SignalR connection when user is authenticated
      signalRService.startConnection();

      // Register notification handler
      const handleNotification = (notification: Notification) => {
        setNotifications((prev) => [notification, ...prev]);
      };

      signalRService.onNotification(handleNotification);

      // Clean up on unmount
      return () => {
        signalRService.offNotification(handleNotification);
        signalRService.stopConnection();
      };
    }
  }, [isAuthenticated]);

  // Calculate unread count
  const unreadCount = notifications.filter((n) => !n.isRead).length;

  // Mark a notification as read
  const markAsRead = (notificationId: number) => {
    setNotifications((prev) =>
      prev.map((n) =>
        n.notificationId === notificationId ? { ...n, isRead: true } : n
      )
    );
  };

  // Mark all notifications as read
  const markAllAsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
  };

  return (
    <NotificationContext.Provider
      value={{ notifications, unreadCount, markAsRead, markAllAsRead }}
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

## Step 3: Create a Notification Component

Create a new file `src/components/NotificationBell.tsx`:

```typescript
import React, { useState } from "react";
import { useNotifications } from "../contexts/NotificationContext";
import {
  Badge,
  IconButton,
  Menu,
  MenuItem,
  Typography,
  Box,
} from "@mui/material";
import NotificationsIcon from "@mui/icons-material/Notifications";
import { formatDistanceToNow } from "date-fns";

const NotificationBell: React.FC = () => {
  const { notifications, unreadCount, markAsRead, markAllAsRead } =
    useNotifications();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNotificationClick = (notificationId: number) => {
    markAsRead(notificationId);
    // You can add navigation logic here based on notification type
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
        {notifications.length === 0 ? (
          <MenuItem disabled>
            <Typography variant="body2">No notifications</Typography>
          </MenuItem>
        ) : (
          notifications.map((notification) => (
            <MenuItem
              key={notification.notificationId}
              onClick={() =>
                handleNotificationClick(notification.notificationId)
              }
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

## Step 4: Add the Notification Provider to Your App

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

## Step 5: Add the Notification Bell to Your Layout

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

## Step 6: Join Course Groups

When a teacher navigates to a specific course page, make sure to join the course-specific SignalR group:

```typescript
import React, { useEffect } from "react";
import { useParams } from "react-router-dom";
import signalRService from "../services/signalRService";

const CoursePage: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();

  useEffect(() => {
    if (courseId) {
      // Join the course group to receive notifications for this course
      signalRService.joinCourseGroup(parseInt(courseId));

      // Clean up when leaving the page
      return () => {
        signalRService.leaveCourseGroup(parseInt(courseId));
      };
    }
  }, [courseId]);

  // Rest of your component...
};
```

## Testing the Notification System

1. Make sure both the backend and frontend are running
2. Log in as a teacher
3. Have a student submit an assignment
4. The teacher should receive a real-time notification without refreshing the page

## Troubleshooting

- Check browser console for any connection errors
- Verify that the SignalR hub URL is correct
- Ensure the JWT token is being passed correctly
- Check that the user has the correct permissions to receive notifications
