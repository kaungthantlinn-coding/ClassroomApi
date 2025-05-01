using Classroom.Dtos.Assignment;

namespace Classroom.Services.Interface;

public interface IAssignmentService
{
    Task<List<AssignmentDto>> GetCourseAssignmentsAsync(int courseId, int userId);
    Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId, int userId);
    Task<AssignmentDto> CreateAssignmentAsync(int courseId, CreateAssignmentDto createAssignmentDto, int teacherId);
    Task<AssignmentDto?> UpdateAssignmentAsync(int assignmentId, UpdateAssignmentDto updateAssignmentDto, int userId);
    Task<bool> DeleteAssignmentAsync(int assignmentId, int userId);
}