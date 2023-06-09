using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validators;
public class CreateRequestValidator : AbstractValidator<CreateRequest> {
    public CreateRequestValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(100);

        RuleFor(comment => comment.Body).NotEmpty();
        RuleFor(comment => comment.Body).MaximumLength(250);
    }
}
