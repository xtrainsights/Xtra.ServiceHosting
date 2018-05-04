using System;

namespace Xtra.ServiceHost.Exceptions
{
    public class MarkedForDeletionException : Exception
    {
        public MarkedForDeletionException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
