namespace Xtra.ServiceHost
{

    public class AADSettings
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CertThumbprint { get; set; }
        public bool Interactive { get; set; } = false;
    }

}
