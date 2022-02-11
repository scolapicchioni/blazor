namespace PhotoSharingApplication.Shared.Validators;

[Serializable]
public class ValidationTrailer {
    public string PropertyName { get; set; }

    public string ErrorMessage { get; set; }

    public string AttemptedValue { get; set; }
}
