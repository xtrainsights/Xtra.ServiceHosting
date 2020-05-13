using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Xtra.ServiceHost.Helpers;


namespace Xtra.ServiceHost
{

    public class AADCredential : TokenCredential
    {
        public AADCredential(AADSettings aadSettings)
        {
            var creds = new List<TokenCredential>();

            if (!String.IsNullOrEmpty(aadSettings.CertThumbprint)) {
                var cert = CertHelper.TryFindCertificate(aadSettings.CertThumbprint);
                creds.Add(new ClientCertificateCredential(aadSettings.TenantId, aadSettings.ClientId, cert));
            } else if (!String.IsNullOrEmpty(aadSettings.ClientSecret)) {
                creds.Add(
                    new ClientSecretCredential(aadSettings.TenantId, aadSettings.ClientId, aadSettings.ClientSecret)
                );
            } else {
                creds.Add(new DefaultAzureCredential(aadSettings.Interactive));
            }

            _innerCredential = new ChainedTokenCredential(creds.ToArray());
        }


        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext,
            CancellationToken cancellationToken)
            => _innerCredential.GetTokenAsync(requestContext, cancellationToken);


        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => _innerCredential.GetToken(requestContext, cancellationToken);


        private readonly TokenCredential _innerCredential;
    }

}
