using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;


namespace Xtra.ServiceHosting.TransformingConfiguration;

public class TransformingConfigurationProvider(IConfigurationRoot root, Func<string, string?, string?> transform)
    : IConfigurationProvider
{
    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        var section = parentPath == null ? _config : _config.GetSection(parentPath);
        var children = section.GetChildren();
        var keys = new List<string>(children.Select(c => c.Key));
        return keys.Concat(earlierKeys).OrderBy(k => k, ConfigurationKeyComparer.Instance);
    }


    public IChangeToken GetReloadToken()
        => _config.GetReloadToken();


    public void Load() { }


    public void Set(string key, string? value)
        => _config[key] = value;


    public bool TryGet(string key, out string? value)
    {
        value = transform(key, _config[key]);
        return value != null;
    }


    private readonly IConfiguration _config = new TransformingConfigurationRoot(root);
}