using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Exceptions;

[Serializable]
public class EditUnauthorizedException<T> : Exception {
    public EditUnauthorizedException() { }
    public EditUnauthorizedException(string message) : base(message) { }
    public EditUnauthorizedException(string message, Exception inner) : base(message, inner) { }
    protected EditUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
