using Classroom.Dtos;
using Classroom.Dtos.Course;

namespace Classroom.Services.Interface;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllCoursesAsync();
    Task<List<CourseDto>> GetUserCoursesAsync(int userId);
    Task<CourseDto?> GetCourseByIdAsync(int courseId, int userId);
    Task<CourseDto?> GetCourseByGuidAsync(Guid courseGuid, int userId);
    Task<CourseDetailDto?> GetCourseDetailByGuidAsync(Guid courseGuid, int userId);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto, int teacherId);
    Task<CourseDto?> UpdateCourseAsync(int courseId, UpdateCourseDto updateCourseDto, int userId);
    Task<bool> DeleteCourseAsync(int courseId, int userId);
    Task<bool> EnrollInCourseAsync(int courseId, string enrollmentCode, int userId);
    Task<bool> EnrollInCourseByCodeAsync(string enrollmentCode, int userId);
    Task<bool> UnenrollFromCourseAsync(int courseId, int userId);
    Task<List<CourseMemberDto>> GetCourseMembersAsync(int courseId, int userId);
    Task<bool> RemoveMemberFromCourseAsync(int courseId, int memberUserId, int requestingUserId);
    Task<bool> IsUserTeacherOfCourseAsync(int courseId, int userId);
    Task<string> GenerateEnrollmentCodeAsync();
    Task<string?> RegenerateEnrollmentCodeAsync(int courseId, int userId);
    Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, int userId);
    Task<bool> UpdateCourseThemeAsync(CourseThemeDto themeDto, int userId);
}