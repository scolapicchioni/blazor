using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validators; 
public class UpdateRequestValidator : AbstractValidator<UpdateRequest> {
    public UpdateRequestValidator() {
        RuleFor(updateRequest => updateRequest.Subject).NotEmpty();
        RuleFor(updateRequest => updateRequest.Subject).MaximumLength(100);

        RuleFor(updateRequest => updateRequest.Body).NotEmpty();
        RuleFor(updateRequest => updateRequest.Body).MaximumLength(250);
    }
}
