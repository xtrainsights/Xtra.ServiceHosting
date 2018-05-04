using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Polly;

using Serilog;

using Xtra.ServiceHost.Exceptions;
using Xtra.ServiceHost.Internals;


namespace Xtra.ServiceHost
{
    public class Bootstrapper
    {

        public Bootstrapper(IServiceConfig config = null, Assembly[] workerAssemblies = null, ISyncPolicy retryPolicy = null)
        {
            _config = config ?? new ServiceConfig();
            _policy = retryPolicy ?? DefaultRetryPolicy;
            _assemblies = workerAssemblies == null || !workerAssemblies.Any()
                ? new[] { Assembly.GetEntryAssembly() }
                : workerAssemblies;
        }


        public int Startup(params string[] args)
        {
            try {
                bool consoleMode;

                using (var serviceManager = new ServiceManager(_config)) {
                    var sm = serviceManager;
                    switch ((args.FirstOrDefault() ?? "").ToLowerInvariant()) {
                        case "/?":
                            Log.Information(
                                "Starts the {AppTitle}.\n\n{AppPath} [/I | /U | /W]",
                                AppMetadata.Title.Value,
                                Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
                            return 0;

                        case "/i":
                        case "/install":
                            var reinstallPolicy = Policy
                                .Handle<AlreadyInstalledException>()
                                .Fallback(() => {
                                    sm.StopService();
                                    sm.UninstallService();
                                    sm.InstallService();
                                });
                            _policy
                                .Wrap(reinstallPolicy)
                                .Execute(() => sm.InstallService());
                            return 0;

                        case "/u":
                        case "/uninstall":
                            sm.UninstallService();
                            return 0;

                        case "/start":
                            sm.StartService();
                            return 0;

                        case "/stop":
                            sm.StopService();
                            return 0;

                        case "/pause":
                            sm.PauseService();
                            return 0;

                        case "/resume":
                            sm.ResumeService();
                            return 0;

                        case "/c":
                        case "/w":
                            consoleMode = true;
                            break;

                        default:
                            consoleMode = Process.GetCurrentProcess().SessionId > 0
                                || AppDomain.CurrentDomain.FriendlyName.EndsWith(".vshost.exe")
                                || Debugger.IsAttached;
                            break;
                    }
                }

                return consoleMode
                    ? RunConsoleMode()
                    : RunServiceMode();

            } catch (Exception ex) {
                Log.Error(ex, "Error bootstrapping services");
                return GetWin32ErrorCode(ex);
            }
        }


        public int RunConsoleMode()
            => new ServiceRunner(_config, _assemblies).RunConsoleMode();


        public int RunServiceMode()
            => new ServiceRunner(_config, _assemblies).RunServiceMode();



        private static int GetWin32ErrorCode(Exception ex)
            => (ex as Win32Exception)?.ErrorCode
               ?? (ex.InnerException as Win32Exception)?.ErrorCode
               ?? -1;


        private readonly Assembly[] _assemblies;
        private readonly IServiceConfig _config;
        private readonly ISyncPolicy _policy;


        private static readonly ISyncPolicy DefaultRetryPolicy
            = Policy
                .Handle<MarkedForDeletionException>()
                .WaitAndRetry(
                    10,
                    x => TimeSpan.FromSeconds(1),
                    (exception, timeSpan, retryCount, context) =>
                        Log.Warning("The specified service has been marked for deletion. Retry attempt {RetryAttempt}", retryCount)
                );


        private static readonly ILogger Log = Serilog.Log.ForContext<Bootstrapper>();

    }
}
