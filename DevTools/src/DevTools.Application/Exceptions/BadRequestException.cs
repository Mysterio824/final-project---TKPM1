using System.Runtime.Serialization;

namespace DevTools.Application.Exceptions
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("Invaid request.") { }

        public BadRequestException(string message)
            : base(message) { }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException) { }

        protected BadRequestException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}