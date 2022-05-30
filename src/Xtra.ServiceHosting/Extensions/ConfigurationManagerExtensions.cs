using Microsoft.Extensions.Configuration;

using Xtra.Models.Settings;


namespace Xtra.ServiceHost.Extensions;

public static class ConfigurationManagerExtensions
{
    public static IConfigurationBuilder AddAzureKeyVault(this ConfigurationManager configurationManager)
        => configurationManager.AddAzureKeyVault(
            configurationManager["KeyVault"], 
            configurationManager.GetSection("AAD").Get<AADSettings>()
        );
}