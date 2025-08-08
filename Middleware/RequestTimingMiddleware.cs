using System.Diagnostics;

namespace CS2_Surf_NET_API.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var watch = Stopwatch.StartNew();
            context.Response.OnStarting(() =>
            {
                watch.Stop();
                context.Response.Headers["X-Response-Time-ms"] = watch.ElapsedMilliseconds.ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
