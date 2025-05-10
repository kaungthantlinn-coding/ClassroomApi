using Classroom.Dtos.Submission;
using Classroom.Dtos.Grade;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using System.Linq;

namespace Classroom.Services.Implementation;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;

    public SubmissionService(
        ISubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository,
        ICourseRepository courseRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
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

    public async Task<SubmissionDto?> UnsubmitSubmissionAsync(int submissionId, int studentId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            return null;
        }

        // Verify the student is the owner of the submission
        if (submission.UserId != studentId)
        {
            throw new UnauthorizedAccessException("You are not authorized to unsubmit this submission");
        }

        // Check if the submission has already been graded
        if (submission.Graded == true)
        {
            throw new InvalidOperationException("Cannot unsubmit a submission that has already been graded");
        }

        // Option 1: Delete the submission entirely
        // This is the most straightforward approach - just remove the submission record
        var deleted = await _submissionRepository.DeleteSubmissionAsync(submissionId);

        if (deleted)
        {
            // Return a placeholder DTO with minimal information since the submission no longer exists
            return new SubmissionDto
            {
                SubmissionId = submissionId,
                AssignmentId = submission.AssignmentId,
                AssignmentTitle = submission.Assignment.Title,
                UserId = studentId,
                UserName = submission.User.Name,
                SubmittedAt = DateTime.MinValue,
                Grade = null,
                Feedback = null,
                Graded = false,
                GradedDate = null,
                SubmissionContent = null
            };
        }

        // If deletion fails for some reason, fall back to the update approach
        // Update submission to unsubmit it
        submission.Content = null; // Remove the content
        submission.SubmittedAt = DateTime.MinValue; // Set to minimum date to indicate it's unsubmitted
        submission.Graded = false; // Ensure it's marked as not graded
        submission.Grade = null; // Remove any grade
        submission.Feedback = null; // Remove any feedback
        submission.GradedDate = null; // Remove graded date

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

    public async Task<GradePageDto> GetCourseGradePageAsync(int courseId, int teacherId)
    {
        // Verify teacher has access to this course
        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to view grades for this course");
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with ID {courseId} not found");
        }

        // Get all assignments for the course
        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdAsync(courseId);

        // Get all submissions for the course
        var submissions = await _submissionRepository.GetCourseSubmissionsAsync(courseId);

        // Get all students in the course
        var courseMembers = await _courseRepository.GetCourseMembersAsync(courseId);
        var students = courseMembers.Where(cm => cm.Role == "Student").ToList();

        // Create student grade DTOs
        var studentGrades = new List<StudentGradeDto>();

        foreach (var student in students)
        {
            var studentSubmissions = submissions.Where(s => s.UserId == student.UserId).ToList();
            var assignmentGrades = new List<AssignmentGradeDto>();

            // Calculate assignment average
            decimal totalGrade = 0;
            int gradedCount = 0;

            foreach (var assignment in assignments)
            {
                var submission = studentSubmissions.FirstOrDefault(s => s.AssignmentId == assignment.AssignmentId);

                var assignmentGrade = new AssignmentGradeDto
                {
                    AssignmentId = assignment.AssignmentId,
                    Title = assignment.Title,
                    DueDate = assignment.DueDate,
                    Points = assignment.Points,
                    Grade = submission?.Grade,
                    Submitted = submission != null,
                    Graded = submission?.Graded ?? false,
                    SubmittedAt = submission?.SubmittedAt
                };

                assignmentGrades.Add(assignmentGrade);

                if (submission?.Graded == true && submission.Grade.HasValue)
                {
                    totalGrade += submission.Grade.Value;
                    gradedCount++;
                }
            }

            // Calculate final grade
            decimal assignmentAverage = gradedCount > 0 ? totalGrade / gradedCount : 0;

            // For now, participation score is just a placeholder (could be based on activity)
            decimal participationScore = 100; // Default full score

            // Final grade is just the assignment average for now
            decimal finalGrade = assignmentAverage;

            var studentGrade = new StudentGradeDto
            {
                UserId = student.UserId,
                Name = student.User.Name,
                Avatar = student.User.Avatar,
                AssignmentAverage = assignmentAverage,
                ParticipationScore = participationScore,
                FinalGrade = finalGrade,
                AssignmentGrades = assignmentGrades
            };

            studentGrades.Add(studentGrade);
        }

        // Calculate class metrics
        var classMetrics = CalculateClassMetrics(studentGrades);

        // Calculate grade distribution
        var gradeDistribution = CalculateGradeDistribution(studentGrades);

        return new GradePageDto
        {
            CourseId = courseId,
            CourseName = course.Name,
            Section = course.Section,
            StudentGrades = studentGrades,
            ClassMetrics = classMetrics,
            GradeDistribution = gradeDistribution
        };
    }

    public async Task<StudentGradeDto?> GetStudentGradeDataAsync(int courseId, int studentId)
    {
        // Verify student is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, studentId);
        if (!isEnrolled)
        {
            throw new UnauthorizedAccessException("You are not enrolled in this course");
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with ID {courseId} not found");
        }

        // Get all assignments for the course
        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdAsync(courseId);

        // Get student's submissions for the course
        var submissions = await _submissionRepository.GetStudentCourseSubmissionsAsync(courseId, studentId);

        // Get student details
        var student = await _courseRepository.GetCourseMemberAsync(courseId, studentId);
        if (student == null)
        {
            return null;
        }

        // Create assignment grade DTOs
        var assignmentGrades = new List<AssignmentGradeDto>();

        // Calculate assignment average
        decimal totalGrade = 0;
        int gradedCount = 0;

        foreach (var assignment in assignments)
        {
            var submission = submissions.FirstOrDefault(s => s.AssignmentId == assignment.AssignmentId);

            var assignmentGrade = new AssignmentGradeDto
            {
                AssignmentId = assignment.AssignmentId,
                Title = assignment.Title,
                DueDate = assignment.DueDate,
                Points = assignment.Points,
                Grade = submission?.Grade,
                Submitted = submission != null,
                Graded = submission?.Graded ?? false,
                SubmittedAt = submission?.SubmittedAt
            };

            assignmentGrades.Add(assignmentGrade);

            if (submission?.Graded == true && submission.Grade.HasValue)
            {
                totalGrade += submission.Grade.Value;
                gradedCount++;
            }
        }

        // Calculate final grade
        decimal assignmentAverage = gradedCount > 0 ? totalGrade / gradedCount : 0;

        // For now, participation score is just a placeholder
        decimal participationScore = 100; // Default full score

        // Final grade is just the assignment average for now
        decimal finalGrade = assignmentAverage;

        return new StudentGradeDto
        {
            UserId = studentId,
            Name = student.User.Name,
            Avatar = student.User.Avatar,
            AssignmentAverage = assignmentAverage,
            ParticipationScore = participationScore,
            FinalGrade = finalGrade,
            AssignmentGrades = assignmentGrades
        };
    }

    private ClassMetricsDto CalculateClassMetrics(List<StudentGradeDto> studentGrades)
    {
        if (!studentGrades.Any())
        {
            return new ClassMetricsDto
            {
                ClassAverage = 0,
                HighestGrade = 0,
                LowestGrade = 0,
                TotalStudents = 0,
                TotalGraded = 0
            };
        }

        var gradedStudents = studentGrades.Where(s => s.AssignmentGrades.Any(a => a.Graded)).ToList();

        return new ClassMetricsDto
        {
            ClassAverage = studentGrades.Any() ? studentGrades.Average(s => s.FinalGrade) : 0,
            HighestGrade = studentGrades.Any() ? studentGrades.Max(s => s.FinalGrade) : 0,
            LowestGrade = studentGrades.Any() ? studentGrades.Min(s => s.FinalGrade) : 0,
            TotalStudents = studentGrades.Count,
            TotalGraded = gradedStudents.Count
        };
    }

    private GradeDistributionDto CalculateGradeDistribution(List<StudentGradeDto> studentGrades)
    {
        var distribution = new GradeDistributionDto();

        foreach (var student in studentGrades)
        {
            if (student.FinalGrade >= 90)
            {
                distribution.ACount++;
            }
            else if (student.FinalGrade >= 80)
            {
                distribution.BCount++;
            }
            else if (student.FinalGrade >= 70)
            {
                distribution.CCount++;
            }
            else if (student.FinalGrade >= 60)
            {
                distribution.DCount++;
            }
            else
            {
                distribution.FCount++;
            }
        }

        return distribution;
    }
}