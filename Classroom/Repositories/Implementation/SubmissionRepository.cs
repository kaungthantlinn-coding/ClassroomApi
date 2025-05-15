using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ClassroomContext _context;

    public SubmissionRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<Submission>> GetAssignmentSubmissionsAsync(int assignmentId)
    {
        return await _context.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.User)
            .Include(s => s.Assignment)
            .Include(s => s.SubmissionAttachments)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
    {
        return await _context.Submissions
            .Include(s => s.User)
            .Include(s => s.Assignment)
            .Include(s => s.SubmissionAttachments)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    public async Task<Submission> CreateSubmissionAsync(Submission submission)
    {
        // Use ExecutionStrategy to handle potential transient errors
        return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            // Use a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                // Check if a submission already exists for this assignment and user
                var existingSubmission = await _context.Submissions
                    .Where(s => s.AssignmentId == submission.AssignmentId && s.UserId == submission.UserId)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefaultAsync();

                if (existingSubmission != null)
                {
                    // Return the existing submission instead of creating a new one
                    await transaction.CommitAsync();
                    return existingSubmission;
                }

                // Create a new submission if none exists
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return submission;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<Submission?> UpdateSubmissionAsync(Submission submission)
    {
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<bool> DeleteSubmissionAsync(int submissionId)
    {
        var submission = await _context.Submissions.FindAsync(submissionId);
        if (submission == null)
        {
            return false;
        }

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserAssignedToAssignmentAsync(int userId, int assignmentId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

        if (assignment == null || assignment.ClassId == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == assignment.ClassId && cm.UserId == userId);
    }

    public async Task<bool> IsTeacherForAssignmentAsync(int teacherId, int assignmentId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

        if (assignment == null || assignment.ClassId == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == assignment.ClassId && cm.UserId == teacherId && cm.Role == "Teacher");
    }

    public async Task<List<Submission>> GetStudentCourseSubmissionsAsync(int courseId, int studentId)
    {
        // Get all assignments for the course
        var assignments = await _context.Assignments
            .Where(a => a.ClassId == courseId && !a.IsDeleted)
            .ToListAsync();

        if (!assignments.Any())
        {
            return new List<Submission>();
        }

        // Get all submissions for these assignments by the student
        var assignmentIds = assignments.Select(a => a.AssignmentId).ToList();

        return await _context.Submissions
            .Where(s => assignmentIds.Contains(s.AssignmentId) && s.UserId == studentId)
            .Include(s => s.Assignment)
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task<List<Submission>> GetCourseSubmissionsAsync(int courseId)
    {
        // Get all assignments for the course
        var assignments = await _context.Assignments
            .Where(a => a.ClassId == courseId && !a.IsDeleted)
            .ToListAsync();

        if (!assignments.Any())
        {
            return new List<Submission>();
        }

        // Get all submissions for these assignments
        var assignmentIds = assignments.Select(a => a.AssignmentId).ToList();

        return await _context.Submissions
            .Where(s => assignmentIds.Contains(s.AssignmentId))
            .Include(s => s.Assignment)
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task<Submission?> GetExistingSubmissionAsync(int assignmentId, int userId)
    {
        // Use ExecutionStrategy to handle potential transient errors
        return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            // Use a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                var submission = await _context.Submissions
                    .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                    .Include(s => s.SubmissionAttachments)
                    .Include(s => s.Assignment)
                    .Include(s => s.User)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefaultAsync();

                await transaction.CommitAsync();
                return submission;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}