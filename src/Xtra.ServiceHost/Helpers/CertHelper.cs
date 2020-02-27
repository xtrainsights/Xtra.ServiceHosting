using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;


namespace Xtra.ServiceHost.Helpers
{

    public static class CertHelper
    {
        #region By Thumbprint

        public static X509Certificate2 TryFindCertificate(string thumbprint)
            => Try(() => FindCertificate(thumbprint));


        public static X509Certificate2 TryLoadCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint)
            => Try(() => LoadCertificate(storeName, storeLocation, thumbprint));


        public static X509Certificate2 FindCertificate(string thumbprint)
            => CertHelper.LoadCertificate(StoreName.My, StoreLocation.CurrentUser, thumbprint)
                ?? CertHelper.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, thumbprint)
                ?? throw new Exception($"A certificate with thumbprint '{thumbprint}' could not be found");


        public static X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            string thumb = ThumbprintCleanerRegex.Replace(thumbprint, String.Empty);
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates
                .Cast<X509Certificate2>()
                .First(xc => thumb.Equals(xc.Thumbprint, StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        #region By Friendly-Name

        public static X509Certificate2 TryFindCertificateByFriendlyName(string name)
            => Try(() => FindCertificateByFriendlyName(name));


        public static X509Certificate2 TryLoadCertificateByFriendlyName(StoreName storeName, StoreLocation storeLocation, string name)
            => Try(() => LoadCertificateByFriendlyName(storeName, storeLocation, name));


        public static X509Certificate2 FindCertificateByFriendlyName(string name)
            => CertHelper.LoadCertificateByFriendlyName(StoreName.My, StoreLocation.CurrentUser, name)
                ?? CertHelper.LoadCertificateByFriendlyName(StoreName.My, StoreLocation.LocalMachine, name)
                ?? throw new Exception($"A certificate with friendly-name '{name}' could not be found");


        public static X509Certificate2 LoadCertificateByFriendlyName(StoreName storeName, StoreLocation storeLocation, string name)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates
                .Cast<X509Certificate2>()
                .First(xc => name.Equals(xc.FriendlyName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        private static T Try<T>(Func<T> func)
        {
            try {
                return func();
            } catch {
                return default(T);
            }
        }


        private static readonly Regex ThumbprintCleanerRegex = new Regex(@"[^abcdef0123456789]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

}
