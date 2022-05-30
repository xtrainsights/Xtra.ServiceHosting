using Microsoft.Extensions.DependencyInjection;

namespace Xtra.ServiceHost.DependencyInjection;

public interface IServiceBundle
{
    void Load(IServiceCollection services);
}
