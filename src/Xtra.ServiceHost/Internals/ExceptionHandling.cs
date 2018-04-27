using System;
using System.ComponentModel;
using System.Reflection;


namespace Xtra.ServiceHost.Internals
{

    internal static class ExceptionHandling
    {

        public static int Handle(Object source, Exception ex, string message)
        {
            if (ex == null) {
                return 0;
            }

            var log = Serilog.Log.ForContext(source.GetType());

            log.Error(ex, message);
            //if (ex.InnerException != null) {
            //    log.Error(ex.InnerException, ex.InnerException.Message);
            //}

            return GetWin32ErrorCode(ex);
        }


        private static int GetWin32ErrorCode(Exception ex)
            => (ex as Win32Exception)?.ErrorCode
                ?? (ex.InnerException as Win32Exception)?.ErrorCode
                ?? -1;

    }

}