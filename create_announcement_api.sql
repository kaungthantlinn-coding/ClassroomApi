-- Create a stored procedure to create an announcement
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CreateAnnouncement')
    DROP PROCEDURE CreateAnnouncement
GO

CREATE PROCEDURE CreateAnnouncement
    @CourseId INT,
    @Content NVARCHAR(MAX),
    @TeacherId INT
AS
BEGIN
    -- Check if the course exists
    IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseId = @CourseId AND IsDeleted = 0)
    BEGIN
        RAISERROR('Course not found', 16, 1)
        RETURN
    END

    -- Check if the user exists
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @TeacherId)
    BEGIN
        RAISERROR('User not found', 16, 1)
        RETURN
    END

    -- Check if the user is a teacher
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @TeacherId AND Role = 'Teacher')
    BEGIN
        RAISERROR('Only teachers can create announcements', 16, 1)
        RETURN
    END

    -- Check if the user is enrolled in the course
    IF NOT EXISTS (SELECT 1 FROM CourseMembers WHERE CourseId = @CourseId AND UserId = @TeacherId)
    BEGIN
        -- Add the user to the course as a teacher
        INSERT INTO CourseMembers (CourseId, UserId, Role)
        VALUES (@CourseId, @TeacherId, 'Teacher')
    END

    -- Check if the user is a teacher in the course
    IF NOT EXISTS (SELECT 1 FROM CourseMembers WHERE CourseId = @CourseId AND UserId = @TeacherId AND Role = 'Teacher')
    BEGIN
        RAISERROR('Only teachers can create announcements for this course', 16, 1)
        RETURN
    END

    -- Get user details
    DECLARE @UserName NVARCHAR(100)
    DECLARE @UserAvatar NVARCHAR(255)

    SELECT @UserName = Name, @UserAvatar = Avatar
    FROM Users
    WHERE UserId = @TeacherId

    -- Create the announcement
    INSERT INTO Announcements (
        AnnouncementGuid,
        ClassId,
        Content,
        AuthorId,
        AuthorName,
        AuthorAvatar,
        CreatedAt,
        IsDeleted
    )
    VALUES (
        NEWID(),
        @CourseId,
        @Content,
        @TeacherId,
        @UserName,
        @UserAvatar,
        GETDATE(),
        0
    )

    -- Return the created announcement
    SELECT TOP 1 *
    FROM Announcements
    WHERE ClassId = @CourseId AND AuthorId = @TeacherId
    ORDER BY CreatedAt DESC
END
GO
