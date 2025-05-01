using Classroom.Dtos.Course;
using FluentValidation;

namespace Classroom.Validators.Course
{
    public class EnrollCourseValidator : AbstractValidator<EnrollCourseDto>
    {
        public EnrollCourseValidator()
        {
            RuleFor(x => x.EnrollmentCode)
                .NotEmpty().WithMessage("Enrollment code is required")
                .MaximumLength(20).WithMessage("Enrollment code cannot exceed 20 characters");
        }
    }
}