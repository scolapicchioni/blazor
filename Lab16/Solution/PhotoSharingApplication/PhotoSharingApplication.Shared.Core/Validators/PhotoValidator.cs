using FluentValidation;
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Validators {
    public class PhotoValidator : AbstractValidator<Photo> {
        public PhotoValidator() {
            RuleFor(photo => photo.Title).NotEmpty();
            RuleFor(photo => photo.Title).MaximumLength(255);

            RuleFor(photo => photo.PhotoImage).SetValidator(new PhotoImageValidator());
        }
    }
}
