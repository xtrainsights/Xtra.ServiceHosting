using Microsoft.Extensions.DependencyInjection;

namespace Xtra.ServiceHosting.DependencyInjection;

public interface IServiceBundle
{
    void Load(IServiceCollection services);
}
