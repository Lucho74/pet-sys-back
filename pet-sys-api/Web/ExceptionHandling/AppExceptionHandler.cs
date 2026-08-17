using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Web.ExceptionHandling
{
    public class AppExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
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
