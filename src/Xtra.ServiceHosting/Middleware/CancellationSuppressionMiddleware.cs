using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;


namespace Xtra.ServiceHosting.Middleware;

/// <summary>
/// This middleware will catch any cancellation exceptions (OperationCanceledException or TaskCanceledException), and return 204 No Content.
/// Adapted from: https://github.com/dotnet/aspnetcore/issues/28568#issuecomment-784013537
/// Relevant Link: https://stackoverflow.com/questions/73791547/get-httpclient-to-abort-the-connection
/// </summary>
internal class CancellationSuppressionMiddleware(RequestDelegate next, IOptions<CancellationSuppressionSettings> options)
{
    public async Task InvokeAsync(HttpContext? httpContext)
    {
        try {
            //Forward to next middleware
            await next(httpContext);

        } catch (OperationCanceledException) {
            //Note TaskCanceledException inherits from OperationCanceledException (so this works for both)
            if (httpContext != null && ShouldHandle(httpContext)) {
                //If we have a response already, then just let that happen.
                if (httpContext.Response.HasStarted) {
                    return;
                }

                //Set a status code. Response will likely not be seen by client as we expect all cancellations to come from httpContext.RequestAborted
                //Why 204 No Content? I could not find any other appropriate status code.
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            } else {
                throw;
            }
        }
    }


    /// <summary>
    /// Examines the HttpContext to determine whether the OperationCanceledException should be handled or re-thrown. Default behaviour is to
    /// check whether the requested path begins with the configured StartsWithPath value.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    protected bool ShouldHandle(HttpContext httpContext)
        => String.IsNullOrEmpty(options.Value.StartsWithPath)
            || httpContext.Request.Path.StartsWithSegments(options.Value.StartsWithPath);
}