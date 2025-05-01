using Classroom.Dtos.Announcement;
using FluentValidation;

namespace Classroom.Validators.Comment
{
    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required")
                .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");
        }
    }
}