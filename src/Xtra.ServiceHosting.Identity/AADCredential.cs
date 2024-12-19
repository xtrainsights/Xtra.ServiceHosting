using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using FluentCertificates;

using Xtra.Models.Settings;


namespace Xtra.ServiceHosting.Identity;

public class AADCredential : TokenCredential
{
    public AADCredential(AADSettings? aadSettings)
    {
        var creds = new List<TokenCredential>();

        if (aadSettings == null) {
            aadSettings = new AADSettings();
        }

        if (aadSettings.Cert != null) {
            creds.Add(new ClientCertificateCredential(aadSettings.TenantId, aadSettings.ClientId, aadSettings.Cert));
        }

        if (!String.IsNullOrEmpty(aadSettings.CertThumbprint)) {
            var result = new CertificateFinder()
                .AddCommonStores()
                .First(x => x.Certificate.Thumbprint.Equals(aadSettings.CertThumbprint, StringComparison.OrdinalIgnoreCase));
            creds.Add(new ClientCertificateCredential(aadSettings.TenantId, aadSettings.ClientId, result.Certificate));
        }

        if (!String.IsNullOrEmpty(aadSettings.ClientSecret)) {
            creds.Add(new ClientSecretCredential(aadSettings.TenantId, aadSettings.ClientId, aadSettings.ClientSecret));
        }

        creds.Add(new DefaultAzureCredential(aadSettings.Interactive));

        _innerCredential = new ChainedTokenCredential(creds.ToArray());
    }


    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => _innerCredential.GetTokenAsync(requestContext, cancellationToken);


    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => _innerCredential.GetToken(requestContext, cancellationToken);


    private readonly TokenCredential _innerCredential;
}
