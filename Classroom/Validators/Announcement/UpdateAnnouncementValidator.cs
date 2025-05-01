using Classroom.Dtos.Announcement;
using FluentValidation;

namespace Classroom.Validators.Announcement
{
    public class UpdateAnnouncementValidator : AbstractValidator<UpdateAnnouncementDto>
    {
        public UpdateAnnouncementValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required")
                .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters");
        }
    }
}