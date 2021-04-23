using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedDeleteAttemptException<T> : Exception {
        public UnauthorizedDeleteAttemptException() {
        }

        public UnauthorizedDeleteAttemptException(string message) : base(message) {
        }

        public UnauthorizedDeleteAttemptException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnauthorizedDeleteAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}