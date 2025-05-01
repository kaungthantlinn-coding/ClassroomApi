using Classroom.Dtos;
using Classroom.Dtos.Announcement;
using Classroom.Dtos.Assignment;
using Classroom.Dtos.Course;
using Classroom.Dtos.Material;
using Classroom.Dtos.Submission;
using FluentValidation;
using FluentValidation.Results;

namespace Classroom.Services.Interface
{
    public interface IValidationService
    {
        // Submission validation
        ValidationResult ValidateCreateSubmission(CreateSubmissionDto dto);
        ValidationResult ValidateGradeSubmission(GradeSubmissionDto dto);
        ValidationResult ValidateFeedbackSubmission(FeedbackSubmissionDto dto);

        // Assignment validation
        ValidationResult ValidateCreateAssignment(CreateAssignmentDto dto);
        ValidationResult ValidateUpdateAssignment(UpdateAssignmentDto dto);

        // Material validation
        ValidationResult ValidateCreateMaterial(CreateMaterialDto dto);
        ValidationResult ValidateUpdateMaterial(UpdateMaterialDto dto);

        // Auth validation
        ValidationResult ValidateRegister(RegisterDto dto);
        ValidationResult ValidateLogin(LoginDto dto);
        ValidationResult ValidateRefreshToken(RefreshTokenDto dto);

        // Announcement validation
        ValidationResult ValidateCreateAnnouncement(CreateAnnouncementDto dto);
        ValidationResult ValidateUpdateAnnouncement(UpdateAnnouncementDto dto);

        // Course validation
        ValidationResult ValidateCreateCourse(CreateCourseDto dto);
        ValidationResult ValidateUpdateCourse(UpdateCourseDto dto);
        ValidationResult ValidateEnrollCourse(EnrollCourseDto dto);

        void ValidateAndThrow<T>(T dto, IValidator<T> validator);
    }
}