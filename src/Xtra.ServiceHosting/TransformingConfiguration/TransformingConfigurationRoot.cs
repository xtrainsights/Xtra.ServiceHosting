using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;


namespace Xtra.ServiceHosting.TransformingConfiguration;

public class TransformingConfigurationRoot : IConfigurationRoot
{
    public TransformingConfigurationRoot(IConfigurationRoot root)
        => _root = new Lazy<ConfigurationRoot>(
            () => {
                var filteredProviders = root.Providers
                    .Where(p => p.GetType() != typeof(TransformingConfigurationProvider))
                    .ToList();
                return new ConfigurationRoot(filteredProviders);
            }
        );


    public IConfigurationSection GetSection(string key)
        => _root.Value.GetSection(key);


    public IEnumerable<IConfigurationSection> GetChildren()
        => _root.Value.GetChildren();


    public IChangeToken GetReloadToken()
        => _root.Value.GetReloadToken();


    public string? this[string key] {
        get => _root.Value[key];
        set => _root.Value[key] = value;
    }


    public IEnumerable<IConfigurationProvider> Providers => _root.Value.Providers;


    public void Reload()
        => _root.Value.Reload();


    private readonly Lazy<ConfigurationRoot> _root;
}
