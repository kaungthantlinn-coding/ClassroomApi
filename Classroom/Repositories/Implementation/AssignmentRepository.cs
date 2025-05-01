using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ClassroomContext _context;

    public AssignmentRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<Assignment>> GetAssignmentsByCourseIdAsync(int courseId)
    {
        return await _context.Assignments
            .Where(a => a.ClassId == courseId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Assignment?> GetByIdAsync(int assignmentId)
    {
        return await _context.Assignments
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    }

    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await SaveChangesAsync();
        return assignment;
    }

    public async Task<Assignment> UpdateAsync(Assignment assignment)
    {
        _context.Assignments.Update(assignment);
        await SaveChangesAsync();
        return assignment;
    }

    public async Task DeleteAsync(Assignment assignment)
    {
        _context.Assignments.Remove(assignment);
        await SaveChangesAsync();
    }

    public async Task<bool> AssignmentExistsAsync(int assignmentId)
    {
        return await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId);
    }

    public async Task<bool> IsUserTeacherOfAssignmentCourseAsync(int assignmentId, int userId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

        if (assignment?.ClassId == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == assignment.ClassId && cm.UserId == userId && cm.Role == "Teacher");
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}