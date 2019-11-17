using System;

namespace Imghoard.Exceptions
{
    public class ResponseException : Exception
    {
        public ResponseException(string reason = null, Exception innerException = null) : base(reason, innerException) { }
    }
}
