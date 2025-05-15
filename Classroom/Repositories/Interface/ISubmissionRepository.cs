using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface ISubmissionRepository
{
    Task<List<Submission>> GetAssignmentSubmissionsAsync(int assignmentId);
    Task<Submission?> GetSubmissionByIdAsync(int submissionId);
    Task<Submission> CreateSubmissionAsync(Submission submission);
    Task<Submission?> UpdateSubmissionAsync(Submission submission);
    Task<bool> DeleteSubmissionAsync(int submissionId);
    Task<bool> IsUserAssignedToAssignmentAsync(int userId, int assignmentId);
    Task<bool> IsTeacherForAssignmentAsync(int teacherId, int assignmentId);
    Task<Submission?> GetExistingSubmissionAsync(int assignmentId, int userId);

    // Grade page related methods
    Task<List<Submission>> GetStudentCourseSubmissionsAsync(int courseId, int studentId);
    Task<List<Submission>> GetCourseSubmissionsAsync(int courseId);
}