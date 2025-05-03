using Classroom.Dtos.Material;
using FluentValidation;

namespace Classroom.Validators.Material
{
    public class CreateMaterialValidator : AbstractValidator<CreateMaterialDto>
    {
        public CreateMaterialValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

            RuleFor(x => x.Topic)
                .MaximumLength(50).WithMessage("Topic cannot exceed 50 characters");

            RuleFor(x => x.ScheduledFor)
                .Matches(@"^\d{4}-\d{2}-\d{2}$").When(x => !string.IsNullOrEmpty(x.ScheduledFor))
                .WithMessage("Scheduled date must be in format YYYY-MM-DD");

            RuleFor(x => x.Color)
                .Must(color => string.IsNullOrEmpty(color) ||
                               System.Text.RegularExpressions.Regex.IsMatch(color, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
                .WithMessage("Color must be a valid hex color code (e.g. #FF5733) or empty for auto-generation");
        }
    }
}