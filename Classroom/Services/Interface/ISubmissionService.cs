using Classroom.Dtos.Submission;

namespace Classroom.Services.Interface;

public interface ISubmissionService
{
    Task<List<SubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId, int teacherId);
    Task<SubmissionDto?> GetSubmissionByIdAsync(int submissionId, int userId);
    Task<SubmissionDto> CreateSubmissionAsync(int assignmentId, CreateSubmissionDto createSubmissionDto, int studentId);
    Task<SubmissionDto?> GradeSubmissionAsync(int submissionId, GradeSubmissionDto gradeSubmissionDto, int teacherId);
    Task<SubmissionDto?> AddFeedbackAsync(int submissionId, FeedbackSubmissionDto feedbackSubmissionDto, int teacherId);
}