using Classroom.Dtos.Course;
using FluentValidation;

namespace Classroom.Validators.Course
{
    public class CourseThemeValidator : AbstractValidator<CourseThemeDto>
    {
        public CourseThemeValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required");

            // No validation for ThemeColor as requested
        }
    }
}
