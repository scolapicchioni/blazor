using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Exceptions;

[Serializable]
public class CreateUnauthorizedException<T> : Exception {
    public CreateUnauthorizedException() { }
    public CreateUnauthorizedException(string message) : base(message) { }
    public CreateUnauthorizedException(string message, Exception inner) : base(message, inner) { }
    protected CreateUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
