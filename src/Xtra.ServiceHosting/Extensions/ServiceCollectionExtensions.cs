using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Xtra.ServiceHosting.DependencyInjection;


namespace Xtra.ServiceHosting.Extensions;

public static class ServiceCollectionExtensions
{
    [Obsolete("Use the AddFactory extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddFactory<T>(this IServiceCollection services)
        where T : class
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddFactory<T>(services);


    [Obsolete("Use the AddFactory extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddFactory<TService, TImplementation>(services);


    [Obsolete("Use the AddFactory extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddFactory<T>(this IServiceCollection services, Func<T> factory)
        where T : class
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddFactory(services, factory);


    [Obsolete("Use the AddServiceBundle extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddServiceBundle<T>(this IServiceCollection services)
        where T : IServiceBundle, new()
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddServiceBundle<T>(services);


    [Obsolete("Use the AddServiceBundle extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddServiceBundle(this IServiceCollection services, IServiceBundle bundle)
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddServiceBundle(services, bundle);


    [Obsolete("Use the AddServiceBundles extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddServiceBundles(this IServiceCollection services, params IServiceBundle[] bundles)
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddServiceBundles(services, bundles);


    [Obsolete("Use the AddServiceBundles extension methods from the Xtra.Extensions.DependencyInjection namespace instead.")]
    public static IServiceCollection AddServiceBundles(this IServiceCollection services, IEnumerable<IServiceBundle> bundles)
        => Xtra.Extensions.DependencyInjection.ServiceCollectionExtensions.AddServiceBundles(services, bundles);
}