using System;

using Microsoft.Extensions.Configuration;


namespace Xtra.ServiceHosting.TransformingConfiguration;

public class TransformingConfigurationSource(IConfigurationRoot root, Func<string, string?, string?> transform)
    : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new TransformingConfigurationProvider(root, transform);
}
