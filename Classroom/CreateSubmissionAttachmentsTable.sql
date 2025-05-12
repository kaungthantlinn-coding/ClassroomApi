-- Create SubmissionAttachments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubmissionAttachments')
BEGIN
    CREATE TABLE [dbo].[SubmissionAttachments] (
        [AttachmentId] INT IDENTITY(1,1) NOT NULL,
        [SubmissionId] INT NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Url] NVARCHAR(255) NOT NULL,
        [Type] NVARCHAR(20) NOT NULL,
        [Size] INT NULL,
        [UploadDate] DATETIME NULL,
        CONSTRAINT [PK__SubmissionA__442C64BE8159921D] PRIMARY KEY CLUSTERED ([AttachmentId] ASC),
        CONSTRAINT [FK__SubmissionA__SubmissionId__5BE2A6F2] FOREIGN KEY ([SubmissionId]) REFERENCES [dbo].[Submissions] ([SubmissionId])
    );
    
    PRINT 'SubmissionAttachments table created successfully.';
END
ELSE
BEGIN
    PRINT 'SubmissionAttachments table already exists.';
END
