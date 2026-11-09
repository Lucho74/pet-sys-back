using Microsoft.AspNetCore.Diagnostics;

namespace Web.ExceptionHandling
{
    public class AppExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                _ => (int?)null
            };

            if (statusCode == null)
            {
                return false;
            }

            httpContext.Response.StatusCode = statusCode.Value;
            await httpContext.Response.WriteAsJsonAsync(new { title = exception.Message }, cancellationToken);
            return true;
        }
    }
}
