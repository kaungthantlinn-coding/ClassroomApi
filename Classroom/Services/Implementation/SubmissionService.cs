using Classroom.Dtos.Submission;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;

namespace Classroom.Services.Implementation;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public SubmissionService(ISubmissionRepository submissionRepository, IAssignmentRepository assignmentRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<List<SubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId, int teacherId)
    {
        // Verify teacher has access to this assignment
        var isTeacher = await _submissionRepository.IsTeacherForAssignmentAsync(teacherId, assignmentId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to view submissions for this assignment");
        }

        var submissions = await _submissionRepository.GetAssignmentSubmissionsAsync(assignmentId);
        return submissions.Select(s => new SubmissionDto
        {
            SubmissionId = s.SubmissionId,
            AssignmentId = s.AssignmentId,
            AssignmentTitle = s.Assignment.Title,
            UserId = s.UserId,
            UserName = s.User.Name,
            SubmittedAt = s.SubmittedAt,
            Grade = s.Grade,
            Feedback = s.Feedback,
            Graded = s.Graded ?? false,
            GradedDate = s.GradedDate,
            SubmissionContent = s.Content
        }).ToList();
    }

    public async Task<SubmissionDto?> GetSubmissionByIdAsync(int submissionId, int userId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            return null;
        }

        // Check if user is the student who submitted or a teacher for the course
        bool hasAccess = submission.UserId == userId ||
                         await _submissionRepository.IsTeacherForAssignmentAsync(userId, submission.AssignmentId);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this submission");
        }

        return new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            AssignmentId = submission.AssignmentId,
            AssignmentTitle = submission.Assignment.Title,
            UserId = submission.UserId,
            UserName = submission.User.Name,
            SubmittedAt = submission.SubmittedAt,
            Grade = submission.Grade,
            Feedback = submission.Feedback,
            Graded = submission.Graded ?? false,
            GradedDate = submission.GradedDate,
            SubmissionContent = submission.Content
        };
    }

    public async Task<SubmissionDto> CreateSubmissionAsync(int assignmentId, CreateSubmissionDto createSubmissionDto, int studentId)
    {
        // Verify assignment exists
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment with ID {assignmentId} not found");
        }

        // Verify student is enrolled in the course
        var isEnrolled = await _submissionRepository.IsUserAssignedToAssignmentAsync(studentId, assignmentId);
        if (!isEnrolled)
        {
            throw new UnauthorizedAccessException("You are not enrolled in this course");
        }

        // Create submission
        var submission = new Submission
        {
            AssignmentId = assignmentId,
            UserId = studentId,
            SubmittedAt = DateTime.UtcNow,
            Content = createSubmissionDto.SubmissionContent,
            Graded = false
        };

        var createdSubmission = await _submissionRepository.CreateSubmissionAsync(submission);

        // Load related entities for the response
        var submissionWithDetails = await _submissionRepository.GetSubmissionByIdAsync(createdSubmission.SubmissionId);

        return new SubmissionDto
        {
            SubmissionId = submissionWithDetails!.SubmissionId,
            AssignmentId = submissionWithDetails.AssignmentId,
            AssignmentTitle = submissionWithDetails.Assignment.Title,
            UserId = submissionWithDetails.UserId,
            UserName = submissionWithDetails.User.Name,
            SubmittedAt = submissionWithDetails.SubmittedAt,
            Grade = submissionWithDetails.Grade,
            Feedback = submissionWithDetails.Feedback,
            Graded = submissionWithDetails.Graded ?? false,
            GradedDate = submissionWithDetails.GradedDate,
            SubmissionContent = submissionWithDetails.Content
        };
    }

    public async Task<SubmissionDto?> GradeSubmissionAsync(int submissionId, GradeSubmissionDto gradeSubmissionDto, int teacherId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            return null;
        }

        // Verify teacher has access to this assignment
        var isTeacher = await _submissionRepository.IsTeacherForAssignmentAsync(teacherId, submission.AssignmentId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to grade this submission");
        }

        // Update submission with grade
        submission.Grade = gradeSubmissionDto.Grade;
        submission.Graded = true;
        submission.GradedDate = DateTime.UtcNow;

        var updatedSubmission = await _submissionRepository.UpdateSubmissionAsync(submission);

        return new SubmissionDto
        {
            SubmissionId = updatedSubmission!.SubmissionId,
            AssignmentId = updatedSubmission.AssignmentId,
            AssignmentTitle = updatedSubmission.Assignment.Title,
            UserId = updatedSubmission.UserId,
            UserName = updatedSubmission.User.Name,
            SubmittedAt = updatedSubmission.SubmittedAt,
            Grade = updatedSubmission.Grade,
            Feedback = updatedSubmission.Feedback,
            Graded = updatedSubmission.Graded ?? false,
            GradedDate = updatedSubmission.GradedDate,
            SubmissionContent = updatedSubmission.Content
        };
    }

    public async Task<SubmissionDto?> AddFeedbackAsync(int submissionId, FeedbackSubmissionDto feedbackSubmissionDto, int teacherId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            return null;
        }

        // Verify teacher has access to this assignment
        var isTeacher = await _submissionRepository.IsTeacherForAssignmentAsync(teacherId, submission.AssignmentId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to add feedback to this submission");
        }

        // Update submission with feedback
        submission.Feedback = feedbackSubmissionDto.Feedback;

        var updatedSubmission = await _submissionRepository.UpdateSubmissionAsync(submission);

        return new SubmissionDto
        {
            SubmissionId = updatedSubmission!.SubmissionId,
            AssignmentId = updatedSubmission.AssignmentId,
            AssignmentTitle = updatedSubmission.Assignment.Title,
            UserId = updatedSubmission.UserId,
            UserName = updatedSubmission.User.Name,
            SubmittedAt = updatedSubmission.SubmittedAt,
            Grade = updatedSubmission.Grade,
            Feedback = updatedSubmission.Feedback,
            Graded = updatedSubmission.Graded ?? false,
            GradedDate = updatedSubmission.GradedDate,
            SubmissionContent = updatedSubmission.Content
        };
    }
}