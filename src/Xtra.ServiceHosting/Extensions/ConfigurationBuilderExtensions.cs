using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xtra.Models.Settings;
using Xtra.ServiceHosting.Identity;
using Xtra.ServiceHosting.TransformingConfiguration;


namespace Xtra.ServiceHosting.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder configurationBuilder, string? keyVault, AADSettings? aadSettings = null)
    {
        if (String.IsNullOrEmpty(keyVault)) {
            return configurationBuilder;
        }

        var keyVaultUri = Uri.IsWellFormedUriString(keyVault, UriKind.Absolute)
            ? new Uri(keyVault)
            : new Uri($"https://{keyVault}.vault.azure.net/");

        var cred = new AADCredential(aadSettings);

        return configurationBuilder.AddAzureKeyVault(keyVaultUri, cred);
    }


    public static IConfigurationBuilder AddEnvironmentOverrides(this IConfigurationBuilder config, IHostEnvironment env, string[] args)
    {
        if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 }) {
            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
            if (appAssembly is not null) {
                config.AddUserSecrets(appAssembly, optional: true, reloadOnChange: true);
            }
        }

        config.AddEnvironmentVariables();

        if (args is { Length: > 0 }) {
            config.AddCommandLine(args);
        }

        return config;
    }


    public static IConfigurationBuilder AddEnvironmentOverrides(this IConfigurationBuilder config, HostBuilderContext ctx, string[] args)
    {
        var env = ctx.HostingEnvironment;
        bool reloadOnChange = GetReloadConfigOnChangeValue(ctx);

        if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 }) {
            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
            if (appAssembly is not null) {
                config.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
            }
        }

        config.AddEnvironmentVariables();

        if (args is { Length: > 0 }) {
            config.AddCommandLine(args);
        }

        return config;
    }


    /// <summary>
    /// Provide a delegate that receives each config key &amp; value and returns a new/transformed config value. One useful
    /// application of this would be dynamically expanding template tags within configuration values.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="transform"></param>
    /// <returns>The transformed/updated value.</returns>
    public static IConfigurationBuilder AddTransformingConfiguration(this IConfigurationBuilder builder, Func<string, string?, string?> transform)
        => builder.Add(new TransformingConfigurationSource(builder.Build(), transform));


    /// <summary>
    /// Provide a delegate that receives each config KeyValuePair and returns a new/transformed config value. One useful
    /// application of this would be dynamically expanding template tags within configuration values.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="transform"></param>
    /// <returns>The transformed/updated value.</returns>
    public static IConfigurationBuilder AddTransformingConfiguration(this IConfigurationBuilder builder, Func<KeyValuePair<string, string?>, string?> transform)
        => AddTransformingConfiguration(builder, (key, value) => transform(KeyValuePair.Create(key, value)));
    

    private static bool GetReloadConfigOnChangeValue(HostBuilderContext hostingContext)
        => hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
}