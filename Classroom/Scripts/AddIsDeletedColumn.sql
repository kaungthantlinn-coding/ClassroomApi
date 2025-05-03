-- Add IsDeleted column to Courses table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Courses]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Courses] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Courses table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Courses table';
END

-- Add IsDeleted column to Assignments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assignments]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Assignments] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Assignments table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Assignments table';
END

-- Add IsDeleted column to Materials table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Materials]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Materials] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Materials table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Materials table';
END

-- Add IsDeleted column to Announcements table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Announcements]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Announcements] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Announcements table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Announcements table';
END

-- Add IsDeleted column to AnnouncementComments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AnnouncementComments]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[AnnouncementComments] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to AnnouncementComments table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in AnnouncementComments table';
END
