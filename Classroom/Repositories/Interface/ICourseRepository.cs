using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<List<Course>> GetCoursesByUserIdAsync(int userId);
    Task<Course?> GetByIdAsync(int courseId);
    Task<Course?> GetByGuidAsync(Guid courseGuid);
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
    Task<List<string>> GetAllEnrollmentCodesAsync();
    Task SaveChangesAsync();

    // New methods for course details
    Task<int> GetCourseMemberCountAsync(int courseId);
    Task<int> GetCourseStudentCountAsync(int courseId);
    Task<int> GetCourseTeacherCountAsync(int courseId);
    Task<int> GetCourseAnnouncementCountAsync(int courseId);
    Task<int> GetCourseAssignmentCountAsync(int courseId);
    Task<int> GetCourseMaterialCountAsync(int courseId);
    Task<List<CourseMember>> GetRecentMembersAsync(int courseId, int count = 5);
    Task<Course?> GetCourseWithDetailsAsync(int courseId);
}