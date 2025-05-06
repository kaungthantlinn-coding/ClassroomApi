-- Insert an announcement directly into the database
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
SELECT
    NEWID(),
    3, -- CourseId
    'This is a test announcement created directly via SQL',
    1004, -- TeacherId
    u.Name,
    u.Avatar,
    GETDATE(),
    0
FROM Users u
WHERE u.UserId = 1004;
