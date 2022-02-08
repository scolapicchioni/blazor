using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class PhotoImageValidator : AbstractValidator<PhotoImage> {
    public PhotoImageValidator() {
        RuleFor(photoImage => photoImage.ImageMimeType).NotEmpty();
        RuleFor(photoImage => photoImage.ImageMimeType).MaximumLength(255);

        RuleFor(photoImage => photoImage.PhotoFile).NotEmpty();
    }
}
