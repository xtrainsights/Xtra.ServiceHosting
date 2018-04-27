using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Serilog;


namespace Xtra.ServiceHost.Internals
{

    internal class AppConsole
    {

        public AppConsole(IEnumerable<Lazy<IAsyncServiceWorker>> workers)
        {
            _workers = workers;
        }


        public async Task<int> RunAsync()
        {
            try {
                Log.Information("Starting {Service}... Press ESC to stop", AppMetadata.Title.Value);
                var tasks = _workers.Select(x => x.Value.OnStart());
                await Task.WhenAll(tasks);

                Log.Information("Started running {Service}", AppMetadata.Title.Value);
                var runningTasks = Task.WhenAll(_workers.Select(x => x.Value.Run()));
                WaitForEscape();

                Log.Information("Stopping {Service}...", AppMetadata.Title.Value);
                await Task.WhenAll(_workers.Select(x => x.Value.OnStop()));

                await runningTasks;

            } catch (OperationCanceledException) {
            } finally {
                Log.Information("Finished running {Service}", AppMetadata.Title.Value);
            }

            return 0;
        }


        private static void WaitForEscape()
        {
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
        }


        private readonly IEnumerable<Lazy<IAsyncServiceWorker>> _workers;


        private static readonly ILogger Log = Serilog.Log.ForContext<AppConsole>();

    }

}