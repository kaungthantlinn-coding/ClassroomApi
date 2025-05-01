using Classroom.Dtos.Announcement;
using FluentValidation;

namespace Classroom.Validators.Comment
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required")
                .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");
        }
    }
}