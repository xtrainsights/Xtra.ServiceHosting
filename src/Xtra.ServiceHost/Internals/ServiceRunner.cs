using System;
using System.Collections.Generic;
using System.Reflection;

using Autofac;

using DasMulli.Win32.ServiceUtils;


namespace Xtra.ServiceHost.Internals
{

    public class ServiceRunner
    {

        public ServiceRunner(params Assembly[] assemblies)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsAssignableTo<IAsyncServiceWorker>() && !t.IsAbstract)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            _container = builder.Build();
        }


        public int RunServiceMode()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var service = new AppService(_container.Resolve<IEnumerable<Lazy<IAsyncServiceWorker>>>());
            return new Win32ServiceHost(service).Run();
        }


        public int RunConsoleMode()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var service = new AppConsole(_container.Resolve<IEnumerable<Lazy<IAsyncServiceWorker>>>());
            return service.RunAsync().GetAwaiter().GetResult();
        }


        private readonly IContainer _container;

    }

}
