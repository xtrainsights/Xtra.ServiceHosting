using System.Collections.Generic;

using DasMulli.Win32.ServiceUtils;


namespace Xtra.ServiceHost
{
    public interface IServiceConfig
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        bool StartImmediately { get; }
        string Username { get; }
        string Password { get; }
        Win32ServiceCredentials DefaultCred { get; }
        List<string> ExtraArguments { get; }
    }
}
