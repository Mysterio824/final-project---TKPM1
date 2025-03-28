using System;
using System.Runtime.Serialization;

namespace DevTools.Domain.Exceptions
{
    [Serializable]
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("You are not authorized to perform this action.") { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException) { }

        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
