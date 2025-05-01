using Classroom.Dtos;
using Classroom.Dtos.Announcement;
using Classroom.Dtos.Assignment;
using Classroom.Dtos.Course;
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

        // Announcement validators
        private readonly IValidator<CreateAnnouncementDto> _createAnnouncementValidator;
        private readonly IValidator<UpdateAnnouncementDto> _updateAnnouncementValidator;

        // Course validators
        private readonly IValidator<CreateCourseDto> _createCourseValidator;
        private readonly IValidator<UpdateCourseDto> _updateCourseValidator;
        private readonly IValidator<EnrollCourseDto> _enrollCourseValidator;

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

            // Announcement validators
            IValidator<CreateAnnouncementDto> createAnnouncementValidator,
            IValidator<UpdateAnnouncementDto> updateAnnouncementValidator,

            // Course validators
            IValidator<CreateCourseDto> createCourseValidator,
            IValidator<UpdateCourseDto> updateCourseValidator,
            IValidator<EnrollCourseDto> enrollCourseValidator)
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

            // Announcement validators
            _createAnnouncementValidator = createAnnouncementValidator;
            _updateAnnouncementValidator = updateAnnouncementValidator;

            // Course validators
            _createCourseValidator = createCourseValidator;
            _updateCourseValidator = updateCourseValidator;
            _enrollCourseValidator = enrollCourseValidator;
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

        // Helper method to throw exception if validation fails
        public void ValidateAndThrow<T>(T dto, IValidator<T> validator)
        {
            validator.ValidateAndThrow(dto);
        }
    }
}