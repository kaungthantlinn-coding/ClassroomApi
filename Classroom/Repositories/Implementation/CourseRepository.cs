using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class CourseRepository : ICourseRepository
{
    private readonly ClassroomContext _context;

    public CourseRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        return await _context.Courses.Where(c => !c.IsDeleted).ToListAsync();
    }

    public async Task<List<Course>> GetCoursesByUserIdAsync(int userId)
    {
        return await _context.CourseMembers
            .Where(cm => cm.UserId == userId)
            .Include(cm => cm.Course)
            .Where(cm => !cm.Course.IsDeleted)
            .Select(cm => cm.Course)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int courseId)
    {
        return await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);
    }

    public async Task<Course> CreateAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await SaveChangesAsync();
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await SaveChangesAsync();
        return course;
    }

    public async Task DeleteAsync(Course course)
    {
        // Implement soft delete
        course.IsDeleted = true;
        _context.Courses.Update(course);
        await SaveChangesAsync();
    }

    public async Task<Course> SoftDeleteAsync(Course course)
    {
        course.IsDeleted = true;
        _context.Courses.Update(course);
        await SaveChangesAsync();
        return course;
    }

    public async Task<bool> AddMemberAsync(CourseMember courseMember)
    {
        await _context.CourseMembers.AddAsync(courseMember);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(CourseMember courseMember)
    {
        _context.CourseMembers.Remove(courseMember);
        await SaveChangesAsync();
        return true;
    }

    public async Task<List<CourseMember>> GetCourseMembersAsync(int courseId)
    {
        return await _context.CourseMembers
            .Where(cm => cm.CourseId == courseId)
            .Include(cm => cm.User)
            .ToListAsync();
    }

    public async Task<CourseMember?> GetCourseMemberAsync(int courseId, int userId)
    {
        return await _context.CourseMembers
            .FirstOrDefaultAsync(cm => cm.CourseId == courseId && cm.UserId == userId);
    }

    public async Task<bool> CourseExistsAsync(int courseId)
    {
        return await _context.Courses.AnyAsync(c => c.CourseId == courseId && !c.IsDeleted);
    }

    public async Task<bool> IsUserEnrolledAsync(int courseId, int userId)
    {
        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == courseId && cm.UserId == userId);
    }

    public async Task<bool> IsUserTeacherOfCourseAsync(int courseId, int userId)
    {
        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == courseId && cm.UserId == userId && cm.Role == "Teacher");
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}