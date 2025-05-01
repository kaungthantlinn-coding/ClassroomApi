using Classroom.Dtos.Submission;
using FluentValidation;

namespace Classroom.Validators.Submission
{
    public class CreateSubmissionValidator : AbstractValidator<CreateSubmissionDto>
    {
        public CreateSubmissionValidator()
        {
            RuleFor(x => x.SubmissionContent)
                .NotEmpty().WithMessage("Submission content is required")
                .MaximumLength(10000).WithMessage("Submission content cannot exceed 10000 characters");
        }
    }
}