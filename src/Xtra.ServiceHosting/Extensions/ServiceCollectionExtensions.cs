using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Xtra.ServiceHosting.DependencyInjection;


namespace Xtra.ServiceHosting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFactory<T>(this IServiceCollection services)
        where T : class
        => services
            .AddTransient<T>()
            .AddSingleton<Func<T>>(x => x.GetRequiredService<T>);


    public static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => services
            .AddTransient<TService, TImplementation>()
            .AddSingleton<Func<TService>>(x => x.GetRequiredService<TService>);


    public static IServiceCollection AddServiceBundle<T>(this IServiceCollection services)
        where T : IServiceBundle, new()
        => LoadBundle(services, Activator.CreateInstance<T>());


    public static IServiceCollection AddServiceBundle(this IServiceCollection services, IServiceBundle bundle)
        => LoadBundle(services, bundle);


    public static IServiceCollection AddServiceBundles(this IServiceCollection services, params IServiceBundle[] bundles)
        => LoadBundles(services, bundles);


    public static IServiceCollection AddServiceBundles(this IServiceCollection services, IEnumerable<IServiceBundle> bundles)
        => LoadBundles(services, bundles);


    private static IServiceCollection LoadBundle<T>(IServiceCollection services, T bundle)
        where T : IServiceBundle
    {
        bundle.Load(services);
        return services;
    }


    private static IServiceCollection LoadBundles(IServiceCollection services, IEnumerable<IServiceBundle> bundles)
    {
        foreach (var bundle in bundles) {
            bundle.Load(services);
        }

        return services;
    }
}