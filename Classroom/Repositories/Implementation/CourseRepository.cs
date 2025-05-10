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

    public async Task<Course?> GetByGuidAsync(Guid courseGuid)
    {
        return await _context.Courses.FirstOrDefaultAsync(c => c.CourseGuid == courseGuid && !c.IsDeleted);
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

    public async Task<List<string>> GetAllEnrollmentCodesAsync()
    {
        return await _context.Courses
            .Where(c => !c.IsDeleted)
            .Select(c => c.EnrollmentCode)
            .ToListAsync();
    }

    public async Task<Course?> GetByEnrollmentCodeAsync(string enrollmentCode)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.EnrollmentCode == enrollmentCode && !c.IsDeleted);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCourseMemberCountAsync(int courseId)
    {
        return await _context.CourseMembers
            .Where(cm => cm.CourseId == courseId)
            .CountAsync();
    }

    public async Task<int> GetCourseStudentCountAsync(int courseId)
    {
        return await _context.CourseMembers
            .Where(cm => cm.CourseId == courseId && cm.Role == "Student")
            .CountAsync();
    }

    public async Task<int> GetCourseTeacherCountAsync(int courseId)
    {
        return await _context.CourseMembers
            .Where(cm => cm.CourseId == courseId && cm.Role == "Teacher")
            .CountAsync();
    }

    public async Task<int> GetCourseAnnouncementCountAsync(int courseId)
    {
        return await _context.Announcements
            .Where(a => a.ClassId == courseId && !a.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetCourseAssignmentCountAsync(int courseId)
    {
        return await _context.Assignments
            .Where(a => a.ClassId == courseId && !a.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetCourseMaterialCountAsync(int courseId)
    {
        return await _context.Materials
            .Where(m => m.ClassId == courseId && !m.IsDeleted)
            .CountAsync();
    }

    public async Task<List<CourseMember>> GetRecentMembersAsync(int courseId, int count = 5)
    {
        return await _context.CourseMembers
            .Where(cm => cm.CourseId == courseId)
            .Include(cm => cm.User)
            .OrderByDescending(cm => cm.UserId) // Using UserId since CourseMember doesn't have an Id field
            .Take(count)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.CourseMembers)
                .ThenInclude(cm => cm.User)
            .Include(c => c.Announcements)
            .Include(c => c.Assignments)
            .Include(c => c.Materials)
            .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);
    }
}