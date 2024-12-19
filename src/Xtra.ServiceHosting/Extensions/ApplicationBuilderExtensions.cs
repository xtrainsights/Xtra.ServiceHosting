using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

using Xtra.ServiceHosting.Middleware;


namespace Xtra.ServiceHosting.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// This middleware will catch any cancellation exceptions (OperationCanceledException or TaskCanceledException), and return 204 No Content.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="startsWithPath">Restricts the cancellation-suppression to path requests which begin with this value. When null (default) or empty, cancellation-suppression will occur on all requests.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseCancellationSuppression(this IApplicationBuilder builder, string? startsWithPath = null)
        => builder.UseMiddleware<CancellationSuppressionMiddleware>(
            Options.Create(new CancellationSuppressionSettings {
                StartsWithPath = startsWithPath
            }));
}
