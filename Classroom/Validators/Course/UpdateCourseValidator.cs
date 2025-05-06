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
                .MaximumLength(20).WithMessage("Enrollment code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.EnrollmentCode));
            // Note: Enrollment code is optional and will only be updated if provided

            RuleFor(x => x.Subject)
                .MaximumLength(50).WithMessage("Subject cannot exceed 50 characters");

            RuleFor(x => x.Room)
                .MaximumLength(20).WithMessage("Room cannot exceed 20 characters");
        }
    }
}