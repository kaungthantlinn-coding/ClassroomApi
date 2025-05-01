using Classroom.Dtos.Submission;
using FluentValidation;

namespace Classroom.Validators.Submission
{
    public class FeedbackSubmissionValidator : AbstractValidator<FeedbackSubmissionDto>
    {
        public FeedbackSubmissionValidator()
        {
            RuleFor(x => x.Feedback)
                .NotEmpty().WithMessage("Feedback is required")
                .MaximumLength(2000).WithMessage("Feedback cannot exceed 2000 characters");
        }
    }
}