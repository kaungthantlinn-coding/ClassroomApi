using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface ICourseRepository
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<List<Course>> GetCoursesByUserIdAsync(int userId);
    Task<Course?> GetByIdAsync(int courseId);
    Task<Course> CreateAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task DeleteAsync(Course course);
    Task<bool> AddMemberAsync(CourseMember courseMember);
    Task<bool> RemoveMemberAsync(CourseMember courseMember);
    Task<List<CourseMember>> GetCourseMembersAsync(int courseId);
    Task<CourseMember?> GetCourseMemberAsync(int courseId, int userId);
    Task<bool> CourseExistsAsync(int courseId);
    Task<bool> IsUserEnrolledAsync(int courseId, int userId);
    Task<bool> IsUserTeacherOfCourseAsync(int courseId, int userId);
    Task SaveChangesAsync();
}