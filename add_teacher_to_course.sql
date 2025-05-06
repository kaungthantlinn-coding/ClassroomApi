-- Check if the teacher is already enrolled in the course
IF NOT EXISTS (
    SELECT 1 
    FROM CourseMembers 
    WHERE CourseId = 3 AND UserId = 1004
)
BEGIN
    -- Add the teacher to the course
    INSERT INTO CourseMembers (CourseId, UserId, Role)
    VALUES (3, 1004, 'Teacher');
    
    PRINT 'Teacher with ID 1004 added to course with ID 3';
END
ELSE
BEGIN
    PRINT 'Teacher with ID 1004 is already enrolled in course with ID 3';
END
