using System.Runtime.Serialization;

namespace DevTools.Application.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("The requested resource was not found.") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }

        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}