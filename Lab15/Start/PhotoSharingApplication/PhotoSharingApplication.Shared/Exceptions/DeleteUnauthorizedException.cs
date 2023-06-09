using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Exceptions; 
[Serializable]
public class DeleteUnauthorizedException<T> : Exception {
    public DeleteUnauthorizedException() { }
    public DeleteUnauthorizedException(string message) : base(message) { }
    public DeleteUnauthorizedException(string message, Exception inner) : base(message, inner) { }
    protected DeleteUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
