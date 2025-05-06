-- Remove Color and TextColor columns from Courses table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Courses]') AND name = 'Color')
BEGIN
    ALTER TABLE [dbo].[Courses] DROP COLUMN [Color];
    PRINT 'Removed Color column from Courses table';
END
ELSE
BEGIN
    PRINT 'Color column does not exist in Courses table';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Courses]') AND name = 'TextColor')
BEGIN
    ALTER TABLE [dbo].[Courses] DROP COLUMN [TextColor];
    PRINT 'Removed TextColor column from Courses table';
END
ELSE
BEGIN
    PRINT 'TextColor column does not exist in Courses table';
END
