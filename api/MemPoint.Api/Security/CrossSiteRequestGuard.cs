using System.Reflection.Metadata.Ecma335;

namespace MemPoint.Api.Security;

public static class CrossSiteRequestGuard
{
    public static IApplicationBuilder UseCrossSiteRequestGuard(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (IsUnsafe(context.Request.Method) && !IsSameOrigin(context.Request))
            {
                await Results.Problem(
                    title: "Cross-site request rejected",
                    statusCode: StatusCodes.Status403Forbidden
                ).ExecuteAsync(context);
                return;
            }

            await next(context);
        });

    private static bool IsUnsafe(string method) =>
        !(HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method));

    private static bool IsSameOrigin(HttpRequest request)
    {
        var site = request.Headers["Sec-Fetch-Site"].ToString();
        if (site.Length > 0) return site is "same-origin" or "none";

        var origin = request.Headers.Origin.ToString();
        if (origin.Length == 0) return true;

        return Uri.TryCreate(origin, UriKind.Absolute, out var uri) && string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase);
    }
}