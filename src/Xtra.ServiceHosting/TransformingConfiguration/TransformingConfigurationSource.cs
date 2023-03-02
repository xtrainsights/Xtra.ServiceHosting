using System;

using Microsoft.Extensions.Configuration;


namespace Xtra.ServiceHosting.TransformingConfiguration;

public class TransformingConfigurationSource : IConfigurationSource
{
    public TransformingConfigurationSource(IConfigurationRoot root, Func<string, string?, string?> transform)
    {
        _root = root;
        _transform = transform;
    }


    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new TransformingConfigurationProvider(_root, _transform);


    private readonly IConfigurationRoot _root;
    private readonly Func<string, string?, string?> _transform;
}
