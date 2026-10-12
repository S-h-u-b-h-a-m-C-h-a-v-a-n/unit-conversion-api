using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Api.ErrorHandling;

/// <summary>
/// Translates domain exceptions into RFC 7807 problem responses. Anything that is not a
/// <see cref="UnitConversionException"/> falls through to the default 500 handler,
/// so internal details are never leaked to callers.
/// </summary>
internal sealed partial class UnitConversionExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<UnitConversionExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnitConversionException conversionException)
        {
            return false;
        }

        var status = GetStatusCode(conversionException);
        LogRejected(logger, conversionException.Title, conversionException.Message);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = conversionException.Title,
                Detail = conversionException.Message,
            },
        });
    }

    // An unknown unit passed as input is a bad request; endpoints that look a unit up by
    // route return 404 themselves. A category is only ever addressed as a resource.
    private static int GetStatusCode(UnitConversionException exception) => exception switch
    {
        CategoryNotFoundException => StatusCodes.Status404NotFound,
        InvalidConversionValueException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest,
    };

    [LoggerMessage(Level = LogLevel.Information, Message = "Conversion request rejected: {Title} - {Detail}")]
    private static partial void LogRejected(ILogger logger, string title, string detail);
}
