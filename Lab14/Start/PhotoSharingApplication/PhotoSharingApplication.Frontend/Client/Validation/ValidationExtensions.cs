using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using PhotoSharingApplication.Shared.Validators;
using System.Text.Json;

namespace PhotoSharingApplication.Frontend.Client.Validation;

public static class ValidationExtensions {
    public static ValidationException ToValidationException(this RpcException ex) {
        var validationTrailer = ex.Trailers.FirstOrDefault(x => x.Key == "validation-errors-text");

        var trailers = JsonSerializer.Deserialize<List<ValidationTrailer>>(validationTrailer.Value);
        List<ValidationFailure> validationFailures = trailers.Select(t => new ValidationFailure(t.PropertyName, t.ErrorMessage, t.AttemptedValue)).ToList();
        throw new FluentValidation.ValidationException(validationFailures);
    }
}
