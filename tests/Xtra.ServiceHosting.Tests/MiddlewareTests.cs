using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Xtra.ServiceHosting.Extensions;
using Xtra.ServiceHosting.Middleware;

using Xunit;


namespace Xtra.ServiceHosting.Tests;

public class MiddlewareTests
{
    [Fact]
    public async Task CancellationSuppressionMiddleware_InvokeAsync_HandlesMatchingPath()
    {
        var middleware = new CancellationSuppressionMiddleware(
            _ => throw new TaskCanceledException(),
            Options.Create(new CancellationSuppressionSettings { StartsWithPath = "/healthz" })
        );

        //Middleware does not catch OperationCanceledException when the requested path does not match /healthz; the exception remains unhandled
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await middleware.InvokeAsync(CreateHttpContextForPath("/")));

        //Middleware catches the OperationCanceledException and returns a 204 status-code when the requested path matches /healthz
        var ctx = CreateHttpContextForPath("/healthz");
        await middleware.InvokeAsync(ctx);
        Assert.Equal(204, ctx.Response.StatusCode);
    }


    private static HttpContext CreateHttpContextForPath(string path)
        => new DefaultHttpContext { Request = { Path = path } };
}