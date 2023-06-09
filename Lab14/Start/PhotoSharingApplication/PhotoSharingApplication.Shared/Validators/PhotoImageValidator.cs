using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;
internal class PhotoImageValidator : AbstractValidator<PhotoImage> {
    public PhotoImageValidator() {
        RuleFor(photoImage => photoImage.ImageMimeType).NotEmpty();
        RuleFor(photoImage => photoImage.ImageMimeType).MaximumLength(255);

        RuleFor(photoImage => photoImage.PhotoFile).NotEmpty();
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
        var result = await ValidateAsync(ValidationContext<PhotoImage>.CreateWithOptions((PhotoImage)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
