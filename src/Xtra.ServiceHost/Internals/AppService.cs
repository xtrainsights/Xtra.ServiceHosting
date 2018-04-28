using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DasMulli.Win32.ServiceUtils;

using Serilog;


namespace Xtra.ServiceHost.Internals
{

    internal class AppService : IWin32Service
    {

        public string ServiceName { get; }

        
        public AppService(string serviceName, IEnumerable<Lazy<IServiceWorker>> workers)
        {
            ServiceName = serviceName;
            Workers = workers;
        }


        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            try {
                Log.Information("Starting {Service}...", ServiceName);
                foreach (var worker in Workers) {
                    ThreadPool.QueueUserWorkItem(state => {
                        async Task RunWorker()
                        {
                            try {
                                await worker.Value.Initialize(startupArguments);
                                await worker.Value.Start();
                            } catch (OperationCanceledException) {
                            } catch (Exception ex) {
                                Log.Error(ex, ex.Message);
                            }
                        }
                        RunWorker().GetAwaiter().GetResult();
                    });
                }

            } catch (Exception ex) {
                Log.Error(ex, ex.Message);
            }
        }


        public void Stop()
        {
            try {
                Log.Information("Stopping {Service}...", ServiceName);
                Task.WhenAll(Workers.Select(x => x.Value.Stop())).GetAwaiter().GetResult();
            } catch (Exception ex) {
                Log.Error(ex, ex.Message);
            }
            Log.Information("Finished running {Service}", ServiceName);
        }


        protected readonly IEnumerable<Lazy<IServiceWorker>> Workers;


        private static readonly ILogger Log = Serilog.Log.ForContext<AppService>();

    }

}
