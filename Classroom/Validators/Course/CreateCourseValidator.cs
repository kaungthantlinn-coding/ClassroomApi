using Classroom.Dtos.Course;
using FluentValidation;

namespace Classroom.Validators.Course
{
    public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
    {
        public CreateCourseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Section)
                .MaximumLength(50).WithMessage("Section cannot exceed 50 characters");

            RuleFor(x => x.TeacherName)
                .MaximumLength(100).WithMessage("Teacher name cannot exceed 100 characters");

            RuleFor(x => x.EnrollmentCode)
                .MaximumLength(20).WithMessage("Enrollment code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.EnrollmentCode));
            // Note: Enrollment code is optional and will be auto-generated if not provided

            RuleFor(x => x.Color)
                .Must(color => string.IsNullOrEmpty(color) ||
                               System.Text.RegularExpressions.Regex.IsMatch(color, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
                .WithMessage("Color must be a valid hex color code (e.g. #FF5733) or empty for auto-generation");

            RuleFor(x => x.TextColor)
                .Must(textColor => string.IsNullOrEmpty(textColor) ||
                                  System.Text.RegularExpressions.Regex.IsMatch(textColor, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
                .WithMessage("Text color must be a valid hex color code (e.g. #FF5733) or empty for auto-generation");

            RuleFor(x => x.Subject)
                .MaximumLength(50).WithMessage("Subject cannot exceed 50 characters");

            RuleFor(x => x.Room)
                .MaximumLength(20).WithMessage("Room cannot exceed 20 characters");
        }
    }
}