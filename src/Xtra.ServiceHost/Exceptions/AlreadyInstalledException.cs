using System;

namespace Xtra.ServiceHost.Exceptions
{
    public class AlreadyInstalledException : Exception
    {
        public AlreadyInstalledException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
