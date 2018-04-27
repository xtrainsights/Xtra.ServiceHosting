using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DasMulli.Win32.ServiceUtils;


namespace Xtra.ServiceHost
{
    public class DefaultConfig : IServiceConfig
    {

        public string Name => AppMetadata.Product.Value;
        public string DisplayName => AppMetadata.Title.Value;
        public string Description => AppMetadata.Description.Value;
        public bool StartImmediately => true;
        public string Username => null;
        public string Password => null;
        public Win32ServiceCredentials DefaultCred => Win32ServiceCredentials.LocalSystem;
        public List<string> ExtraArguments { get; } = new List<string>();

    }
}
