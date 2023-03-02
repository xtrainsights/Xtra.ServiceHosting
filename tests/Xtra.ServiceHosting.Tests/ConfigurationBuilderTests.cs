using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Xtra.Models.Settings;
using Xtra.ServiceHosting.Extensions;

using Xunit;
using Xunit.Abstractions;


namespace Xtra.ServiceHosting.Tests;

public class ConfigurationBuilderTests
{
    public ConfigurationBuilderTests(ITestOutputHelper output)
        => Output = output;


    [Fact]
    public void ConfigurationBuilder_AddAzureKeyVaultWithAADSettings_RegistersProvider()
    {
        var baseConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(TestData)
            .Build();

        var settings = baseConfig.Get<CommonSettingsBase>();

        var builder = new ConfigurationBuilder()
            .AddAzureKeyVault(settings?.KeyVault, settings?.AAD);

        Assert.NotEmpty(builder.Sources);
    }


    [Fact]
    public void ConfigurationBuilder_AddTransformingConfiguration_OutputsModifiedConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(TestData)
            .AddTransformingConfiguration(BasicTemplateExpander)
            .Build();

        Assert.Equal("Test App v1.0", config["AppTitle"]);
    }


    [Fact]
    public void ConfigurationBuilder_AddTransformingConfiguration_OnlyModifiesPriorConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(TestData)
            .AddTransformingConfiguration(BasicTemplateExpander)
            .AddInMemoryCollection(TestData) //This config provider is unaffected by the TransformingConfiguration above
            .Build();

        Assert.Equal("Test App {Version}", config["AppTitle"]);
    }


    private static string? BasicTemplateExpander(string key, string? value)
        => value?.Replace("{Version}", "v1.0");


    private ITestOutputHelper Output { get; init; }


    private static readonly Dictionary<string, string?> TestData = new() {
        { "KeyVault", "Vault-Test" },
        { "AAD:TenantId", "FakeTenant" },
        { "AAD:ClientId", Guid.Empty.ToString() },
        { "AAD:ClientSecret", "FakeSecret" },
        { "AppTitle", "Test App {Version}" }
    };
}