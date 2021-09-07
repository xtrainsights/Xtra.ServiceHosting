using System;

using Microsoft.Extensions.Configuration;

using Xtra.Models.Settings;
using Xtra.Common.Identity;


namespace Xtra.ServiceHost.Extensions
{

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder self, string keyVault, AADSettings aadSettings)
        {
            if (String.IsNullOrEmpty(keyVault)) {
                return self;
            }

            var keyVaultUri = Uri.IsWellFormedUriString(keyVault, UriKind.Absolute)
                ? new Uri(keyVault)
                : new Uri($"https://{keyVault}.vault.azure.net/");

            var cred = new AADCredential(aadSettings);

            self.AddAzureKeyVault(keyVaultUri, cred);

            return self;
        }
    }

}
