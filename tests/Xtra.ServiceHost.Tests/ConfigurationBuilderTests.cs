using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Xtra.Models.Settings;
using Xtra.ServiceHost.Extensions;

using Xunit;
using Xunit.Abstractions;


namespace Xtra.ServiceHost.Tests;

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

        var settings = baseConfig.Get<CommonSettingsBase>()!;

        var builder = new ConfigurationBuilder()
            .AddAzureKeyVault(settings.KeyVault, settings.AAD);

        Assert.NotEmpty(builder.Sources);
    }


    private ITestOutputHelper Output { get; init; }


    private static readonly Dictionary<string, string?> TestData = new() {
        { "KeyVault", "Vault-Test" },
        { "AAD:TenantId", "FakeTenant" },
        { "AAD:ClientId", Guid.Empty.ToString() },
        { "AAD:ClientSecret", "FakeSecret" }
    };
}