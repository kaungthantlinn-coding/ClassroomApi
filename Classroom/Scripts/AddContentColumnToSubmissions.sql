-- Add Content column to Submissions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Submissions]') AND name = 'Content')
BEGIN
    ALTER TABLE [dbo].[Submissions] ADD [Content] NVARCHAR(MAX) NULL;
    PRINT 'Added Content column to Submissions table';
END
ELSE
BEGIN
    PRINT 'Content column already exists in Submissions table';
END
