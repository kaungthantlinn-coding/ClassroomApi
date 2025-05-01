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
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
    {
        return await _context.Submissions
            .Include(s => s.User)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    public async Task<Submission> CreateSubmissionAsync(Submission submission)
    {
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<Submission?> UpdateSubmissionAsync(Submission submission)
    {
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
        return submission;
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
}