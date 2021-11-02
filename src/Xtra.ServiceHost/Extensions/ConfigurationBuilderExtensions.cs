using System;

using Microsoft.Extensions.Configuration;

using Xtra.Models.Settings;
using Xtra.Common.Identity;


namespace Xtra.ServiceHost.Extensions
{

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder configurationBuilder, string keyVault, AADSettings aadSettings = null)
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
    }

}
