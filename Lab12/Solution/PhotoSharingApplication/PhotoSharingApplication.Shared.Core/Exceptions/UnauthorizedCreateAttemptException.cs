using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedCreateAttemptException<T> : Exception {
        public UnauthorizedCreateAttemptException() { }
        public UnauthorizedCreateAttemptException(string message) : base(message) { }
        public UnauthorizedCreateAttemptException(string message, Exception inner) : base(message, inner) { }
        protected UnauthorizedCreateAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
