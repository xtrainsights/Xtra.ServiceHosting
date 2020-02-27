using System;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

using Xtra.ServiceHost.Helpers;


namespace Xtra.ServiceHost.Extensions
{

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder ConfigureAzureKeyVault(this IConfigurationBuilder self, string keyVault, AADSettings aadSettings)
        {
            if (String.IsNullOrEmpty(keyVault)) {
                return self;
            }

            if (!String.IsNullOrEmpty(aadSettings.CertThumbprint)) {
                var cert = CertHelper.TryFindCertificate(aadSettings.CertThumbprint);
                self.AddAzureKeyVault(keyVault, aadSettings.ClientId, cert);
            } else if (!String.IsNullOrEmpty(aadSettings.ClientSecret)) {
                self.AddAzureKeyVault(keyVault, aadSettings.ClientId, aadSettings.ClientSecret);
            } else {
                var tokenProvider = new AzureServiceTokenProvider();
                var tokenCallback = new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback);
                var kvc = new KeyVaultClient(tokenCallback);
                self.AddAzureKeyVault(keyVault, kvc, new DefaultKeyVaultSecretManager());
            }

            return self;
        }
    }

}
