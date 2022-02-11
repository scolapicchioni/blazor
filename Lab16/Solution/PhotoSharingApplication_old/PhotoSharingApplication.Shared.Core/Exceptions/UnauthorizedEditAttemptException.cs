using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedEditAttemptException<T> : Exception {
        public UnauthorizedEditAttemptException() {
        }

        public UnauthorizedEditAttemptException(string message) : base(message) {
        }

        public UnauthorizedEditAttemptException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnauthorizedEditAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}