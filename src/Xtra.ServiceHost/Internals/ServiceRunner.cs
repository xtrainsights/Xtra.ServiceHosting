using System;
using System.Collections.Generic;
using System.Reflection;

using Autofac;

using DasMulli.Win32.ServiceUtils;


namespace Xtra.ServiceHost.Internals
{

    internal class ServiceRunner
    {

        public ServiceRunner(IServiceConfig config, params Assembly[] assemblies)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsAssignableTo<IServiceWorker>() && !t.IsAbstract)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            _container = builder.Build();
            _config = config;
        }


        public int RunServiceMode()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var workers = _container.Resolve<IEnumerable<Lazy<IServiceWorker>>>();

            var host = _config.Pausable
                ? new Win32ServiceHost(new AppPausableService(_config.Name, workers))
                : new Win32ServiceHost(new AppService(_config.Name, workers));

            return host.Run();
        }


        public int RunConsoleMode()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var workers = _container.Resolve<IEnumerable<Lazy<IServiceWorker>>>();
            var service = new AppConsole(_config.Name, workers);
            return service.RunAsync().GetAwaiter().GetResult();
        }


        private readonly IServiceConfig _config;
        private readonly IContainer _container;

    }

}
