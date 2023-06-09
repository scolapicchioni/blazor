using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;
public class CommentValidator : AbstractValidator<Comment> {
    public CommentValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(100);

        RuleFor(comment => comment.Body).NotEmpty();
        RuleFor(comment => comment.Body).MaximumLength(250);
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
        var result = await ValidateAsync(ValidationContext<Comment>.CreateWithOptions((Comment)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
