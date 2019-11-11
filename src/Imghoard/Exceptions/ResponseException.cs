using System;

namespace Imghoard.Exceptions
{
    public class ResponseException : Exception
    {
        public ResponseException(string Reason = null, Exception innerException = null) : base(Reason, innerException) { }
    }
}
