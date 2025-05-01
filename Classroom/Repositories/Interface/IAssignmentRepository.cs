using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IAssignmentRepository
{
    Task<List<Assignment>> GetAssignmentsByCourseIdAsync(int courseId);
    Task<Assignment?> GetByIdAsync(int assignmentId);
    Task<Assignment> CreateAsync(Assignment assignment);
    Task<Assignment> UpdateAsync(Assignment assignment);
    Task DeleteAsync(Assignment assignment);
    Task<bool> AssignmentExistsAsync(int assignmentId);
    Task<bool> IsUserTeacherOfAssignmentCourseAsync(int assignmentId, int userId);
    Task SaveChangesAsync();
}