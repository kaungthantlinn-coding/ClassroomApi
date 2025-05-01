# Classroom API

## Overview

Classroom API is a comprehensive backend solution for managing educational environments. It provides a robust set of features for course management, announcements, assignments, materials, and user interactions in a classroom setting.

## Features

- **User Management**: Registration, authentication, and profile management
- **Course Management**: Create, join, and manage courses
- **Announcements**: Post and manage course announcements with comments
- **Assignments**: Create, submit, and grade assignments
- **Materials**: Share and organize course materials
- **Comments**: Interactive discussions on announcements

## Technology Stack

- **Framework**: ASP.NET Core
- **Database**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Language**: C#

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (or compatible database)
- Visual Studio 2022 or compatible IDE

### Installation

1. Clone the repository

   ```
   git clone https://github.com/yourusername/classroom-api.git
   cd classroom-api
   ```

2. Configure the database connection in `appsettings.json`

3. Apply database migrations

   ```
   dotnet ef database update
   ```

4. Run the application
   ```
   dotnet run
   ```

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/refresh` - Refresh JWT token

### Courses

- `GET /api/courses` - Get all courses for current user
- `GET /api/courses/{id}` - Get course details
- `POST /api/courses` - Create a new course
- `PUT /api/courses/{id}` - Update course details
- `DELETE /api/courses/{id}` - Delete a course

### Announcements

- `GET /api/courses/{courseId}/announcements` - Get course announcements
- `GET /api/announcements/{id}` - Get announcement details
- `POST /api/courses/{courseId}/announcements` - Create announcement
- `PUT /api/announcements/{id}` - Update announcement
- `DELETE /api/announcements/{id}` - Delete announcement

### Comments

- `GET /api/announcements/{announcementId}/comments` - Get comments
- `POST /api/announcements/{announcementId}/comments` - Create comment
- `PUT /api/comments/{id}` - Update comment
- `DELETE /api/comments/{id}` - Delete comment

### Assignments

- `GET /api/courses/{courseId}/assignments` - Get course assignments
- `GET /api/assignments/{id}` - Get assignment details
- `POST /api/courses/{courseId}/assignments` - Create assignment
- `PUT /api/assignments/{id}` - Update assignment
- `DELETE /api/assignments/{id}` - Delete assignment

### Submissions

- `GET /api/assignments/{assignmentId}/submissions` - Get submissions
- `POST /api/assignments/{assignmentId}/submissions` - Submit assignment
- `PUT /api/submissions/{id}` - Update submission

### Materials

- `GET /api/courses/{courseId}/materials` - Get course materials
- `GET /api/materials/{id}` - Get material details
- `POST /api/courses/{courseId}/materials` - Upload material
- `DELETE /api/materials/{id}` - Delete material

## Authentication

The API uses JWT (JSON Web Tokens) for authentication. To access protected endpoints:

1. Obtain a token by logging in via `/api/auth/login`
2. Include the token in the Authorization header of subsequent requests:
   ```
   Authorization: Bearer {your_token}
   ```
3. Use the refresh token endpoint `/api/auth/refresh` to get a new access token when needed

## Error Handling

The API returns standard HTTP status codes along with a JSON response:

```json
{
  "status": 400,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

## Development

### Project Structure

- **Controllers/**: API endpoints
- **Models/**: Database entities
- **Dtos/**: Data transfer objects
- **Services/**: Business logic
- **Repositories/**: Data access layer

### Adding Migrations

```
dotnet ef migrations add MigrationName
dotnet ef database update
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for the ORM capabilities
- All contributors to this project
