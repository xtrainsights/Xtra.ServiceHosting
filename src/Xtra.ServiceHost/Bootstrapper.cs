using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

using DasMulli.Win32.ServiceUtils;

using Microsoft.Extensions.PlatformAbstractions;

using Serilog;

using Xtra.ServiceHost.Internals;


namespace Xtra.ServiceHost
{
    public class Bootstrapper
    {

        public Bootstrapper()
            : this(new DefaultConfig(), Assembly.GetEntryAssembly())
        {
        }


        public Bootstrapper(IServiceConfig config, params Assembly[] assemblies)
        {
            _config = config;
            _assemblies = assemblies;
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
                            InstallService(sc, _config);
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

                    case "/c":
                    case "/w":
                        consoleMode = true;
                        break;

                    default:
                        Log.Debug("Interactive {Value}", Environment.UserInteractive);
                        Log.Debug("IsAttached {Value}", Debugger.IsAttached);
                        Log.Debug("AppDomain {Value}", AppDomain.CurrentDomain.FriendlyName);
                        consoleMode = Environment.UserInteractive || Debugger.IsAttached || AppDomain.CurrentDomain.FriendlyName.EndsWith(".vshost.exe");
                        consoleMode = false;
                        break;
                }

                return consoleMode
                    ? RunConsoleMode()
                    : RunServiceMode();

            } catch (Exception ex) {
                return ExceptionHandling.Handle(this, ex, "Error bootstrapping services");
            }
        }


        public int RunConsoleMode()
            => new ServiceRunner(_assemblies).RunConsoleMode();


        public int RunServiceMode()
            => new ServiceRunner(_assemblies).RunServiceMode();


        private static void ReinstallService(ServiceController sc, IServiceConfig config)
        {
            StopService(sc, config);
            UninstallService(sc, config);
            InstallService(sc, config);
        }


        private static void InstallService(ServiceController sc, IServiceConfig config, int counter = 0)
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
                Log.Information("Service {Service} was already installed. Reinstalling...", config.Name);
                ReinstallService(sc, config);

            } catch (Exception e) when (e.Message.Contains("The specified service has been marked for deletion")) {
                if (counter < 10) {
                    Thread.Sleep(500);
                    counter++;
                    string suffix = "th";
                    switch (counter) {
                        case 1: suffix = "st"; break;
                        case 2: suffix = "nd"; break;
                        case 3: suffix = "rd"; break;
                    }
                    Log.Warning("The specified service has been marked for deletion. Retrying {Attempt} time", counter + suffix);
                    InstallService(sc, config, counter);
                    return;
                }
                throw;
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
                Log.Information("Service {Service} is already paused or stop is pending.", config.Name);
            }
        }


        private static void ContinueService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.ContinuePending) {
                sc.Continue();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(1000));
                Log.Information("Successfully stopped service {Service}", config.Name);
            } else {
                Log.Information("Service {Service} is already stopped or stop is pending.", config.Name);
            }
        }


        private static void StartService(ServiceController sc, IServiceConfig config)
        {
            if (sc.Status != ServiceControllerStatus.StartPending && sc.Status != ServiceControllerStatus.Running) {
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


        private readonly IServiceConfig _config;
        private readonly Assembly[] _assemblies;


        private static readonly ILogger Log = Serilog.Log.ForContext<Bootstrapper>();

    }
}
