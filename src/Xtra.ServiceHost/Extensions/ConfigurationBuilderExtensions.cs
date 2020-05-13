using System;
using System.Collections.Generic;

using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Configuration;

using Xtra.ServiceHost.Helpers;


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

            var creds = new List<TokenCredential>();

            if (!String.IsNullOrEmpty(aadSettings.CertThumbprint)) {
                var cert = CertHelper.TryFindCertificate(aadSettings.CertThumbprint);
                creds.Add(new ClientCertificateCredential(aadSettings.TenantId, aadSettings.ClientId, cert));

            } else if (!String.IsNullOrEmpty(aadSettings.ClientSecret)) {
                creds.Add(new ClientSecretCredential(aadSettings.TenantId, aadSettings.ClientId, aadSettings.ClientSecret));

            } else {
                creds.Add(new DefaultAzureCredential(aadSettings.Interactive));
            }

            var cred = new ChainedTokenCredential(creds.ToArray());

            self.AddAzureKeyVault(keyVaultUri, cred);

            return self;
        }
    }

}
