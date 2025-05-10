using Classroom.Dtos.Submission;
using Classroom.Dtos.Grade;

namespace Classroom.Services.Interface;

public interface ISubmissionService
{
    Task<List<SubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId, int teacherId);
    Task<SubmissionDto?> GetSubmissionByIdAsync(int submissionId, int userId);
    Task<SubmissionDto> CreateSubmissionAsync(int assignmentId, CreateSubmissionDto createSubmissionDto, int studentId);
    Task<SubmissionDto?> GradeSubmissionAsync(int submissionId, GradeSubmissionDto gradeSubmissionDto, int teacherId);
    Task<SubmissionDto?> AddFeedbackAsync(int submissionId, FeedbackSubmissionDto feedbackSubmissionDto, int teacherId);
    Task<SubmissionDto?> UnsubmitSubmissionAsync(int submissionId, int studentId);

    // Grade page related methods
    Task<GradePageDto> GetCourseGradePageAsync(int courseId, int teacherId);
    Task<StudentGradeDto?> GetStudentGradeDataAsync(int courseId, int studentId);
}