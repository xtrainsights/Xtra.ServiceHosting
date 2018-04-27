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

        
        public AppService(IEnumerable<Lazy<IAsyncServiceWorker>> workers)
        {
            ServiceName = AppMetadata.Title.Value;
            _workers = workers;
        }


        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            try {
                Log.Information("Starting {Service}...", AppMetadata.Title.Value);
                foreach (var worker in _workers) {
                    ThreadPool.QueueUserWorkItem(state => {
                        async Task RunWorker()
                        {
                            try {
                                await worker.Value.OnStart(startupArguments);
                                await worker.Value.Run();
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
                Log.Information("Stopping {Service}...", AppMetadata.Title.Value);
                Task.WhenAll(_workers.Select(x => x.Value.OnStop())).GetAwaiter().GetResult();
            } catch (Exception ex) {
                Log.Error(ex, ex.Message);
            }
            Log.Information("Finished running {Service}", AppMetadata.Title.Value);
        }


        private readonly IEnumerable<Lazy<IAsyncServiceWorker>> _workers;


        private static readonly ILogger Log = Serilog.Log.ForContext<AppService>();

    }

}
