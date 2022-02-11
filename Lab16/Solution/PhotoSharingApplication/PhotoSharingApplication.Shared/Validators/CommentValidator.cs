using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class CommentValidator : AbstractValidator<Comment> {
    public CommentValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(250);

        RuleFor(comment => comment.Body).NotEmpty();
    }
}
