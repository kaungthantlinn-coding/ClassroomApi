using Classroom.Dtos.Assignment;
using FluentValidation;

namespace Classroom.Validators.Assignment
{
    public class CreateAssignmentValidator : AbstractValidator<CreateAssignmentDto>
    {
        public CreateAssignmentValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

            RuleFor(x => x.Instructions)
                .MaximumLength(5000).WithMessage("Instructions cannot exceed 5000 characters");

            RuleFor(x => x.Points)
                .MaximumLength(10).WithMessage("Points value is too large");

            RuleFor(x => x.DueDate)
                .Matches(@"^\d{4}-\d{2}-\d{2}$").When(x => !string.IsNullOrEmpty(x.DueDate))
                .WithMessage("Due date must be in format YYYY-MM-DD");

            RuleFor(x => x.DueTime)
                .Matches(@"^\d{2}:\d{2}$").When(x => !string.IsNullOrEmpty(x.DueTime))
                .WithMessage("Due time must be in format HH:MM");

            RuleFor(x => x.Topic)
                .MaximumLength(50).WithMessage("Topic cannot exceed 50 characters");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) ||
                               status == "Draft" ||
                               status == "Published" ||
                               status == "Archived")
                .WithMessage("Status must be either 'Draft', 'Published', or 'Archived'");

            RuleFor(x => x.LateSubmissionPolicy)
                .MaximumLength(500).WithMessage("Late submission policy cannot exceed 500 characters");
        }
    }
}