using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Serilog;


namespace Xtra.ServiceHost.Internals
{

    internal class AppConsole
    {

        public string ServiceName { get; }


        public AppConsole(string serviceName, IEnumerable<Lazy<IServiceWorker>> workers)
        {
            ServiceName = serviceName;
            Workers = workers;
        }


        public async Task<int> RunAsync()
        {
            try {
                Log.Information("Starting {Service}... Press ESC to stop", ServiceName);
                var tasks = Workers.Select(x => x.Value.Initialize());
                await Task.WhenAll(tasks);

                Log.Information("Started running {Service}", ServiceName);
                var runningTasks = Task.WhenAll(Workers.Select(x => x.Value.Start()));
                WaitForEscape();

                Log.Information("Stopping {Service}...", ServiceName);
                await Task.WhenAll(Workers.Select(x => x.Value.Stop()));

                await runningTasks;

            } catch (OperationCanceledException) {
            } finally {
                Log.Information("Finished running {Service}", ServiceName);
            }

            return 0;
        }


        private static void WaitForEscape()
        {
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
        }


        protected readonly IEnumerable<Lazy<IServiceWorker>> Workers;


        private static readonly ILogger Log = Serilog.Log.ForContext<AppConsole>();

    }

}