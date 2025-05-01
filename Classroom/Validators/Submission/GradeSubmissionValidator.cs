using Classroom.Dtos.Submission;
using FluentValidation;

namespace Classroom.Validators.Submission
{
    public class GradeSubmissionValidator : AbstractValidator<GradeSubmissionDto>
    {
        public GradeSubmissionValidator()
        {
            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required")
                .InclusiveBetween(0, 100).WithMessage("Grade must be between 0 and 100");
        }
    }
}