using Classroom.Dtos.Course;
using FluentValidation;

namespace Classroom.Validators.Course
{
    public class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Section)
                .MaximumLength(50).WithMessage("Section cannot exceed 50 characters");

            RuleFor(x => x.TeacherName)
                .MaximumLength(100).WithMessage("Teacher name cannot exceed 100 characters");

            RuleFor(x => x.EnrollmentCode)
                .NotEmpty().WithMessage("Enrollment code is required")
                .MaximumLength(20).WithMessage("Enrollment code cannot exceed 20 characters");

            RuleFor(x => x.Color)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").When(x => !string.IsNullOrEmpty(x.Color))
                .WithMessage("Color must be a valid hex color code (e.g. #FF5733)");

            RuleFor(x => x.TextColor)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").When(x => !string.IsNullOrEmpty(x.TextColor))
                .WithMessage("Text color must be a valid hex color code (e.g. #FF5733)");

            RuleFor(x => x.Subject)
                .MaximumLength(50).WithMessage("Subject cannot exceed 50 characters");

            RuleFor(x => x.Room)
                .MaximumLength(20).WithMessage("Room cannot exceed 20 characters");
        }
    }
}