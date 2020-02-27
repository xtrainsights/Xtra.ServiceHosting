using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Xtra.ServiceHost
{

    public abstract class WorkerService<T> : BackgroundService
    {
        protected WorkerService(IOptionsMonitor<T> settings, ILogger<WorkerService<T>> logger)
        {
            Settings = settings;
            Log = logger;
            settings.OnChange(LoadSettings);
        }


        protected virtual void LoadSettings(T settings) { }


        protected IOptionsMonitor<T> Settings { get; }
        protected ILogger Log { get; }
    }

}
