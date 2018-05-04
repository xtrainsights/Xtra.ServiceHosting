using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

using DasMulli.Win32.ServiceUtils;

using Microsoft.Extensions.PlatformAbstractions;

using Polly;

using Serilog;

using Xtra.ServiceHost.Exceptions;
using Xtra.ServiceHost.Internals;


namespace Xtra.ServiceHost
{
    public class Bootstrapper
    {

        public Bootstrapper()
            : this(new ServiceConfig(), DefaultRetryPolicy)
        {
        }


        public Bootstrapper(IServiceConfig config, ISyncPolicy retryPolicy, params Assembly[] workerAssemblies)
        {
            _policy = retryPolicy;
            _config = config;
            _assemblies = !workerAssemblies.Any()
                ? new[] { Assembly.GetEntryAssembly() }
                : workerAssemblies;
        }


        public int Startup(params string[] args)
        {
            try {
                bool consoleMode;

                switch ((args.FirstOrDefault() ?? "").ToLowerInvariant()) {
                    case "/?":
                        Log.Information("Starts the {AppTitle}.\n\n{AppPath} [/I | /U | /W]",
                            AppMetadata.Title.Value,
                            Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
                        return 0;

                    case "/i":
                    case "/install":
                        using (var sc = new ServiceController(_config.Name)) {
                            var ctrl = sc;
                            var reinstallPolicy = Policy
                                .Handle<AlreadyInstalledException>()
                                .Fallback(() => {
                                    StopService(ctrl, _config);
                                    UninstallService(ctrl, _config);
                                    InstallService(ctrl, _config);
                                });
                            reinstallPolicy.Wrap(_policy).Execute(() => InstallService(ctrl, _config));
                            return 0;
                        }

                    case "/u":
                    case "/uninstall":
                        using (var sc = new ServiceController(_config.Name)) {
                            UninstallService(sc, _config);
                            return 0;
                        }

                    case "/start":
                        using (var sc = new ServiceController(_config.Name)) {
                            StartService(sc, _config);
                            return 0;
                        }

                    case "/stop":
                        using (var sc = new ServiceController(_config.Name)) {
                            StopService(sc, _config);
                            return 0;
                        }

                    case "/pause":
                        using (var sc = new ServiceController(_config.Name)) {
                            PauseService(sc, _config);
                            return 0;
                        }

                    case "/resume":
                        using (var sc = new ServiceController(_config.Name)) {
                            ResumeService(sc, _config);
                            return 0;
                        }

                    case "/c":
                    case "/w":
                        consoleMode = true;
                        break;

                    default:
                        consoleMode = Process.GetCurrentProcess().SessionId > 0 || Debugger.IsAttached || AppDomain.CurrentDomain.FriendlyName.EndsWith(".vshost.exe");
                        break;
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


        private static void InstallService(ServiceController sc, IServiceConfig config)
        {
            try {
                var serviceDef = new ServiceDefinition(config.Name, GetServiceCommand(config.ExtraArguments)) {
                    Description = config.Description,
                    AutoStart = true,
                    DisplayName = config.DisplayName,
                    Credentials = !String.IsNullOrEmpty(config.Username)
                        ? new Win32ServiceCredentials(config.Username, config.Password)
                        : config.DefaultCred
                };

                var manager = new Win32ServiceManager();
                manager.CreateService(serviceDef, config.StartImmediately);
                Log.Information("Successfully installed {Service}", config.Name);

            } catch (Exception e) when (e.Message.Contains("already exists")) {
                throw new AlreadyInstalledException(e);

            } catch (Exception e) when (e.Message.Contains("The specified service has been marked for deletion")) {
                throw new MarkedForDeletionException(e);
            }
        }


        private static void UninstallService(ServiceController sc, IServiceConfig config)
        {
            try {
                if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending) {
                    StopService(sc, config);
                }

                new Win32ServiceManager().DeleteService(config.Name);
                Log.Information("Successfully unregistered service {Service}", config.Name);

            } catch (InvalidOperationException e) when (e.Message.Contains("was not found on computer")) {
                Log.Warning("Service {Service} does not exist. No action taken.", config.Name);

            } catch (Exception e) when (e.Message.Contains("does not exist")) {
                Log.Warning("Service {Service} does not exist. No action taken.", config.Name);
            }
        }


        private static void StopService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending) {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(1000));
                Log.Information("Successfully stopped service {Service}", config.Name);
            } else {
                Log.Information("Service {Service} is already stopped or stop is pending.", config.Name);
            }
        }


        private static void PauseService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.Paused && sc.Status != ServiceControllerStatus.PausePending) {
                sc.Pause();
                sc.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMilliseconds(1000));
                Log.Information("Successfully paused service {Service}", config.Name);
            } else {
                Log.Information("Service {Service} is already paused or pause is pending.", config.Name);
            }
        }


        private static void ResumeService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.ContinuePending) {
                sc.Continue();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(1000));
                Log.Information("Successfully resumed service {Service}", config.Name);
            } else {
                Log.Information("Service {Service} is already running or continue is pending.", config.Name);
            }
        }


        private static void StartService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending) {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(1000));
                Log.Information("Successfully started service {Service}", config.Name);
            } else {
                Log.Information("Service {Service} is already running or start is pending.", config.Name);
            }
        }


        private static string GetServiceCommand(List<string> extraArguments)
        {
            var host = Process.GetCurrentProcess().MainModule.FileName;

            if (host.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase)) {
                var app = PlatformServices.Default.Application;
                var appPath = Path.Combine(app.ApplicationBasePath, app.ApplicationName + ".dll");
                host = $"\"{host}\" \"{appPath}\"";

            } else {
                //For self-contained apps, skip the dll path
                extraArguments = extraArguments.Skip(1).ToList();
            }

            return $"{host} {String.Join(" ", extraArguments)}";
        }


        private static int GetWin32ErrorCode(Exception ex)
            => (ex as Win32Exception)?.ErrorCode
               ?? (ex.InnerException as Win32Exception)?.ErrorCode
               ?? -1;


        private readonly IServiceConfig _config;
        private readonly Assembly[] _assemblies;
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
