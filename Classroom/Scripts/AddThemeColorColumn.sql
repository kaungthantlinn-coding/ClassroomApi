-- Add ThemeColor column to Courses table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Courses]') AND name = 'ThemeColor')
BEGIN
    ALTER TABLE [dbo].[Courses] ADD [ThemeColor] NVARCHAR(20) NULL;
    PRINT 'Added ThemeColor column to Courses table';
END
ELSE
BEGIN
    PRINT 'ThemeColor column already exists in Courses table';
END
