using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Xtra.ServiceHost.DependencyInjection;
using Xtra.ServiceHost.Extensions;

using Xunit;


namespace Xtra.ServiceHost.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public void ServiceCollection_AddServiceBundle_WithGenerics()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundle<FooBundle>();
        var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundle_WithInstance()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundle(new FooBundle());
        var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundles_WithParams()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundles(new FooBundle(), new BarBundle());
        var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
        Assert.IsType<BarService>(sp.GetRequiredService<BarService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundles_WithList()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundles(new List<IServiceBundle> { new FooBundle(), new BarBundle() });
        var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
        Assert.IsType<BarService>(sp.GetRequiredService<BarService>());
    }


    private class FooBundle : IServiceBundle
    {
        public void Load(IServiceCollection services) => services.AddTransient<FooService>();
    }


    private class BarBundle : IServiceBundle
    {
        public void Load(IServiceCollection services) => services.AddTransient<BarService>();
    }


    private class FooService { }


    private class BarService { }
}