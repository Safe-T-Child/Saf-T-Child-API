public class SelectiveHttpsMiddleware
{
    private readonly RequestDelegate _next;

    public SelectiveHttpsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;

        // Check if the request is for an endpoint that should not be redirected to HTTPS
        if (request.Path.ToString().Contains("http-only-endpoint"))
        {
            // Proceed without redirecting to HTTPS
            await _next(context);
            return;
        }

        // For all other requests, enforce HTTPS
        if (!request.IsHttps)
        {
            // Instead of redirecting, return an error response
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsync("HTTPS required for this endpoint.");
            return;
        }

        await _next(context);
    }
}
