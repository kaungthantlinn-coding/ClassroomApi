-- Execute the stored procedure to create an announcement
EXEC CreateAnnouncement
    @CourseId = 3,
    @Content = 'Test announcement created via stored procedure',
    @TeacherId = 1004
