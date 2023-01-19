using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Xtra.ServiceHosting.Extensions;
using Xtra.ServiceHosting.DependencyInjection;

using Xunit;


namespace Xtra.ServiceHosting.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public void ServiceCollection_AddFactory_OfImplementation()
    {
        var sc = new ServiceCollection();
        sc.AddFactory<FooService>();

        using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<Func<FooService>>();

        Assert.IsType<FooService>(factory());
        Assert.NotSame(factory(), factory());
    }


    [Fact]
    public void ServiceCollection_AddFactory_OfInterface()
    {
        var sc = new ServiceCollection();
        sc.AddFactory<ITestService, FooService>();

        using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<Func<ITestService>>();

        Assert.IsType<FooService>(factory());
        Assert.NotSame(factory(), factory());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundle_WithGenerics()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundle<FooBundle>();

        using var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundle_WithInstance()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundle(new FooBundle());

        using var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundles_WithParams()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundles(new FooBundle(), new BarBundle());

        using var sp = sc.BuildServiceProvider();
        Assert.IsType<FooService>(sp.GetRequiredService<FooService>());
        Assert.IsType<BarService>(sp.GetRequiredService<BarService>());
    }


    [Fact]
    public void ServiceCollection_AddServiceBundles_WithList()
    {
        var sc = new ServiceCollection();
        sc.AddServiceBundles(new List<IServiceBundle> { new FooBundle(), new BarBundle() });

        using var sp = sc.BuildServiceProvider();
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


    private interface ITestService { }


    private class FooService : ITestService { }


    private class BarService : ITestService { }
}