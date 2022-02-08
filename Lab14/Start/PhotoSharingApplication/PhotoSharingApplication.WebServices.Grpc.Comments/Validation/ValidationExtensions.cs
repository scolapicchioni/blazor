using Grpc.Core;
using PhotoSharingApplication.Shared.Validators;
using System.Text.Json;
using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validation;

public static class ValidationExtensions {
    public static RpcException ToRpcException(this ValidationException ex) {
        var metadata = new Metadata();
        List<ValidationTrailer> trailers = ex.Errors.Select(x => new ValidationTrailer {
            PropertyName = x.PropertyName,
            AttemptedValue = x.AttemptedValue?.ToString(),
            ErrorMessage = x.ErrorMessage
        }).ToList();
        string json = JsonSerializer.Serialize(trailers);
        metadata.Add(new Metadata.Entry("validation-errors-text", json));
        return new RpcException(new Status(StatusCode.InvalidArgument, "Validation Failed"), metadata);
    }
}
