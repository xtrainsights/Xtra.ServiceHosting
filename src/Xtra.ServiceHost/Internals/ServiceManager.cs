using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

using DasMulli.Win32.ServiceUtils;

using Microsoft.Extensions.PlatformAbstractions;

using Serilog;

using Xtra.ServiceHost.Exceptions;


namespace Xtra.ServiceHost.Internals
{
    internal class ServiceManager : ServiceController
    {

        public ServiceManager(IServiceConfig config)
            : base(config.Name)
        {
            _config = config;
        }


        public void InstallService()
        {
            try {
                var failureActionRestart = new ScAction { Type = ScActionType.ScActionRestart };

                var serviceDef = new ServiceDefinition(_config.Name, GetServiceCommand(_config.ExtraArguments)) {
                    Description = _config.Description,
                    AutoStart = true,
                    DisplayName = _config.DisplayName,
                    FailureActions = new ServiceFailureActions(TimeSpan.Zero, null, null, new []{ failureActionRestart }),
                    Credentials = !String.IsNullOrEmpty(_config.Username)
                        ? new Win32ServiceCredentials(_config.Username, _config.Password)
                        : _config.DefaultCred
                };

                InteropServiceManager.CreateService(serviceDef, _config.StartImmediately);
                Log.Information("Successfully installed {Service}", _config.Name);

            } catch (Exception e) when (e.Message.Contains("already exists")) {
                throw new AlreadyInstalledException(e);

            } catch (Exception e) when (e.Message.Contains("The specified service has been marked for deletion")) {
                throw new MarkedForDeletionException(e);
            }
        }


        public void UninstallService()
        {
            try {
                if (Status != ServiceControllerStatus.Stopped && Status != ServiceControllerStatus.StopPending) {
                    StopService();
                }

                InteropServiceManager.DeleteService(_config.Name);
                Log.Information("Successfully removed service {Service}", _config.Name);

            } catch (InvalidOperationException e) when (e.Message.Contains("was not found on computer")) {
                Log.Warning("Service {Service} does not exist. No action taken.", _config.Name);

            } catch (Exception e) when (e.Message.Contains("does not exist")) {
                Log.Warning("Service {Service} does not exist. No action taken.", _config.Name);
            }
        }


        public void StopService()
        {
            if (Status != ServiceControllerStatus.Stopped && Status != ServiceControllerStatus.StopPending) {
                Stop();
                WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(10000));
                Log.Information("Successfully stopped service {Service}", _config.Name);
            } else {
                Log.Information("Service {Service} is already stopped or stop is pending.", _config.Name);
            }
        }


        public void PauseService()
        {
            if (Status != ServiceControllerStatus.Paused && Status != ServiceControllerStatus.PausePending) {
                Pause();
                WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMilliseconds(10000));
                Log.Information("Successfully paused service {Service}", _config.Name);
            } else {
                Log.Information("Service {Service} is already paused or pause is pending.", _config.Name);
            }
        }


        public void ResumeService()
        {
            if (Status != ServiceControllerStatus.Running && Status != ServiceControllerStatus.ContinuePending) {
                Continue();
                WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(10000));
                Log.Information("Successfully resumed service {Service}", _config.Name);
            } else {
                Log.Information("Service {Service} is already running or continue is pending.", _config.Name);
            }
        }


        public void StartService()
        {
            if (Status != ServiceControllerStatus.Running && Status != ServiceControllerStatus.StartPending) {
                Start();
                WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(10000));
                Log.Information("Successfully started service {Service}", _config.Name);
            } else {
                Log.Information("Service {Service} is already running or start is pending.", _config.Name);
            }
        }


        public string GetServiceCommand(List<string> extraArguments)
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


        private IServiceConfig _config;


        private static readonly Win32ServiceManager InteropServiceManager = new Win32ServiceManager();


        private static readonly ILogger Log = Serilog.Log.ForContext<Bootstrapper>();

    }
}
