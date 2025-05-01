using Classroom.Dtos.Announcement;
using FluentValidation;

namespace Classroom.Validators.Announcement
{
    public class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementDto>
    {
        public CreateAnnouncementValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required")
                .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters");
        }
    }
}