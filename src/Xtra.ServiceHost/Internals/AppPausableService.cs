using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DasMulli.Win32.ServiceUtils;

using Serilog;


namespace Xtra.ServiceHost.Internals
{

    internal class AppPausableService : AppService, IPausableWin32Service
    {

        public AppPausableService(string serviceName, IEnumerable<Lazy<IServiceWorker>> workers)
            : base(serviceName, workers)
        {
        }


        public void Pause()
        {
            try {
                Log.Information("Pausing {Service}...", ServiceName);
                Task.WhenAll(Workers.Select(x => x.Value.Pause())).GetAwaiter().GetResult();
            } catch (Exception ex) {
                Log.Error(ex, ex.Message);
            }
            Log.Information("Paused {Service}", ServiceName);
        }


        public void Continue()
        {
            try {
                Log.Information("Continuing {Service}...", ServiceName);
                Task.WhenAll(Workers.Select(x => x.Value.Resume())).GetAwaiter().GetResult();
            } catch (Exception ex) {
                Log.Error(ex, ex.Message);
            }
        }


        private static readonly ILogger Log = Serilog.Log.ForContext<AppService>();

    }

}
