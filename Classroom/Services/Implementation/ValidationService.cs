using Classroom.Dtos;
using Classroom.Dtos.Announcement;
using Classroom.Dtos.Assignment;
using Classroom.Dtos.Course;
using Classroom.Dtos.Email;
using Classroom.Dtos.Material;
using Classroom.Dtos.Submission;
using Classroom.Services.Interface;
using FluentValidation;
using FluentValidation.Results;

namespace Classroom.Services.Implementation
{
    public class ValidationService : IValidationService
    {
        // Submission validators
        private readonly IValidator<CreateSubmissionDto> _createSubmissionValidator;
        private readonly IValidator<GradeSubmissionDto> _gradeSubmissionValidator;
        private readonly IValidator<FeedbackSubmissionDto> _feedbackSubmissionValidator;

        // Assignment validators
        private readonly IValidator<CreateAssignmentDto> _createAssignmentValidator;
        private readonly IValidator<UpdateAssignmentDto> _updateAssignmentValidator;

        // Material validators
        private readonly IValidator<CreateMaterialDto> _createMaterialValidator;
        private readonly IValidator<UpdateMaterialDto> _updateMaterialValidator;

        // Auth validators
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RefreshTokenDto> _refreshTokenValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

        // Announcement validators
        private readonly IValidator<CreateAnnouncementDto> _createAnnouncementValidator;
        private readonly IValidator<UpdateAnnouncementDto> _updateAnnouncementValidator;

        // Course validators
        private readonly IValidator<CreateCourseDto> _createCourseValidator;
        private readonly IValidator<UpdateCourseDto> _updateCourseValidator;
        private readonly IValidator<EnrollCourseDto> _enrollCourseValidator;

        // Comment validators
        private readonly IValidator<CreateCommentDto> _createCommentValidator;
        private readonly IValidator<UpdateCommentDto> _updateCommentValidator;

        // Email validators
        private readonly IValidator<CourseInvitationDto> _courseInvitationValidator;
        private readonly IValidator<BulkCourseInvitationDto> _bulkCourseInvitationValidator;

        public ValidationService(
            // Submission validators
            IValidator<CreateSubmissionDto> createSubmissionValidator,
            IValidator<GradeSubmissionDto> gradeSubmissionValidator,
            IValidator<FeedbackSubmissionDto> feedbackSubmissionValidator,

            // Assignment validators
            IValidator<CreateAssignmentDto> createAssignmentValidator,
            IValidator<UpdateAssignmentDto> updateAssignmentValidator,

            // Material validators
            IValidator<CreateMaterialDto> createMaterialValidator,
            IValidator<UpdateMaterialDto> updateMaterialValidator,

            // Auth validators
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<RefreshTokenDto> refreshTokenValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,

            // Announcement validators
            IValidator<CreateAnnouncementDto> createAnnouncementValidator,
            IValidator<UpdateAnnouncementDto> updateAnnouncementValidator,

            // Course validators
            IValidator<CreateCourseDto> createCourseValidator,
            IValidator<UpdateCourseDto> updateCourseValidator,
            IValidator<EnrollCourseDto> enrollCourseValidator,

            // Comment validators
            IValidator<CreateCommentDto> createCommentValidator,
            IValidator<UpdateCommentDto> updateCommentValidator,

            // Email validators
            IValidator<CourseInvitationDto> courseInvitationValidator,
            IValidator<BulkCourseInvitationDto> bulkCourseInvitationValidator)
        {
            // Submission validators
            _createSubmissionValidator = createSubmissionValidator;
            _gradeSubmissionValidator = gradeSubmissionValidator;
            _feedbackSubmissionValidator = feedbackSubmissionValidator;

            // Assignment validators
            _createAssignmentValidator = createAssignmentValidator;
            _updateAssignmentValidator = updateAssignmentValidator;

            // Material validators
            _createMaterialValidator = createMaterialValidator;
            _updateMaterialValidator = updateMaterialValidator;

            // Auth validators
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _refreshTokenValidator = refreshTokenValidator;
            _changePasswordValidator = changePasswordValidator;

            // Announcement validators
            _createAnnouncementValidator = createAnnouncementValidator;
            _updateAnnouncementValidator = updateAnnouncementValidator;

            // Course validators
            _createCourseValidator = createCourseValidator;
            _updateCourseValidator = updateCourseValidator;
            _enrollCourseValidator = enrollCourseValidator;

            // Comment validators
            _createCommentValidator = createCommentValidator;
            _updateCommentValidator = updateCommentValidator;

            // Email validators
            _courseInvitationValidator = courseInvitationValidator;
            _bulkCourseInvitationValidator = bulkCourseInvitationValidator;
        }

        public ValidationResult ValidateCreateSubmission(CreateSubmissionDto dto)
        {
            return _createSubmissionValidator.Validate(dto);
        }

        public ValidationResult ValidateGradeSubmission(GradeSubmissionDto dto)
        {
            return _gradeSubmissionValidator.Validate(dto);
        }

        public ValidationResult ValidateFeedbackSubmission(FeedbackSubmissionDto dto)
        {
            return _feedbackSubmissionValidator.Validate(dto);
        }

        public ValidationResult ValidateCreateAssignment(CreateAssignmentDto dto)
        {
            return _createAssignmentValidator.Validate(dto);
        }

        public ValidationResult ValidateUpdateAssignment(UpdateAssignmentDto dto)
        {
            return _updateAssignmentValidator.Validate(dto);
        }

        public ValidationResult ValidateCreateMaterial(CreateMaterialDto dto)
        {
            return _createMaterialValidator.Validate(dto);
        }

        public ValidationResult ValidateUpdateMaterial(UpdateMaterialDto dto)
        {
            return _updateMaterialValidator.Validate(dto);
        }

        // Auth validation methods
        public ValidationResult ValidateRegister(RegisterDto dto)
        {
            return _registerValidator.Validate(dto);
        }

        public ValidationResult ValidateLogin(LoginDto dto)
        {
            return _loginValidator.Validate(dto);
        }

        public ValidationResult ValidateRefreshToken(RefreshTokenDto dto)
        {
            return _refreshTokenValidator.Validate(dto);
        }

        public ValidationResult ValidateChangePassword(ChangePasswordDto dto)
        {
            return _changePasswordValidator.Validate(dto);
        }

        // Announcement validation methods
        public ValidationResult ValidateCreateAnnouncement(CreateAnnouncementDto dto)
        {
            return _createAnnouncementValidator.Validate(dto);
        }

        public ValidationResult ValidateUpdateAnnouncement(UpdateAnnouncementDto dto)
        {
            return _updateAnnouncementValidator.Validate(dto);
        }

        // Course validation methods
        public ValidationResult ValidateCreateCourse(CreateCourseDto dto)
        {
            return _createCourseValidator.Validate(dto);
        }

        public ValidationResult ValidateUpdateCourse(UpdateCourseDto dto)
        {
            return _updateCourseValidator.Validate(dto);
        }

        public ValidationResult ValidateEnrollCourse(EnrollCourseDto dto)
        {
            return _enrollCourseValidator.Validate(dto);
        }

        // Comment validation methods
        public ValidationResult ValidateCreateComment(CreateCommentDto dto)
        {
            return _createCommentValidator.Validate(dto);
        }

        public ValidationResult ValidateUpdateComment(UpdateCommentDto dto)
        {
            return _updateCommentValidator.Validate(dto);
        }

        // Email validation methods
        public ValidationResult ValidateCourseInvitation(CourseInvitationDto dto)
        {
            return _courseInvitationValidator.Validate(dto);
        }

        public ValidationResult ValidateBulkCourseInvitation(BulkCourseInvitationDto dto)
        {
            return _bulkCourseInvitationValidator.Validate(dto);
        }

        // Helper method to throw exception if validation fails
        public void ValidateAndThrow<T>(T dto, IValidator<T> validator)
        {
            validator.ValidateAndThrow(dto);
        }
    }
}