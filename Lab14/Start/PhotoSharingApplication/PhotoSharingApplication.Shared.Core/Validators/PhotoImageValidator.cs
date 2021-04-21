using FluentValidation;
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Validators {
    public class PhotoImageValidator : AbstractValidator<PhotoImage> {
        public PhotoImageValidator() {
            RuleFor(photoImage => photoImage.ImageMimeType).NotEmpty();
            RuleFor(photoImage => photoImage.ImageMimeType).MaximumLength(255);

            RuleFor(photoImage => photoImage.PhotoFile).NotEmpty();
        }
    }
}