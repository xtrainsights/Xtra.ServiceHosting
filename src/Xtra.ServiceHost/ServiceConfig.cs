using System.Collections.Generic;
using DasMulli.Win32.ServiceUtils;
using Xtra.ServiceHost.Internals;

namespace Xtra.ServiceHost
{
    public class ServiceConfig : IServiceConfig
    {
        public virtual string Name { get; set; } = AppMetadata.Product.Value;
        public virtual string DisplayName { get; set; } = AppMetadata.Title.Value;
        public virtual string Description { get; set; } = AppMetadata.Description.Value;
        public virtual bool StartImmediately { get; set; } = true;
        public virtual bool Pausable { get; set; } = false;
        public virtual string Username { get; set; } = null;
        public virtual string Password { get; set; } = null;
        public virtual Win32ServiceCredentials DefaultCred { get; set; } = Win32ServiceCredentials.LocalSystem;
        public virtual List<string> ExtraArguments { get; set; } = new List<string>();
    }
}