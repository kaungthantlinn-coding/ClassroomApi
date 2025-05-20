using Classroom.Dtos.Email;
using FluentValidation;

namespace Classroom.Validators.Email
{
    public class CourseInvitationValidator : AbstractValidator<CourseInvitationDto>
    {
        public CourseInvitationValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID must be greater than 0");

            RuleFor(x => x.RecipientEmail)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.CustomMessage)
                .MaximumLength(1000).WithMessage("Custom message cannot exceed 1000 characters");
        }
    }

    public class BulkCourseInvitationValidator : AbstractValidator<BulkCourseInvitationDto>
    {
        public BulkCourseInvitationValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID must be greater than 0");

            RuleFor(x => x.RecipientEmails)
                .NotEmpty().WithMessage("At least one email address is required");

            RuleForEach(x => x.RecipientEmails)
                .NotEmpty().WithMessage("Email address cannot be empty")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.CustomMessage)
                .MaximumLength(1000).WithMessage("Custom message cannot exceed 1000 characters");
        }
    }
}
