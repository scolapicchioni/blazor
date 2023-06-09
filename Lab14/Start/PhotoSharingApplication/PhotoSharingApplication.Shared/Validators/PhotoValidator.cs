using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;
public class PhotoValidator : AbstractValidator<Photo> {
    public PhotoValidator() {
        RuleFor(photo => photo.Title).NotEmpty();
        RuleFor(photo => photo.Title).MaximumLength(100);

        RuleFor(photo => photo.Description).MaximumLength(255);
        
        RuleFor(photo => photo.PhotoImage).SetValidator(new PhotoImageValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
        var result = await ValidateAsync(ValidationContext<Photo>.CreateWithOptions((Photo)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
