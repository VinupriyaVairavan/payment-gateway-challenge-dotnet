using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.ApplicationInsights;

using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.ErrorHandler;

public class GlobalErrorHandlingMiddleware(
    RequestDelegate next,
    ILoggerService<GlobalErrorHandlingMiddleware> logger//,
    //TelemetryClient telemetryClient
    )
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred while processing the request");

            // telemetryClient.TrackException(ex, new Dictionary<string, string>
            // {
            //     { "TraceId", activity?.TraceId.ToString() ?? "N/A" },
            //     { "SpanId", activity?.SpanId.ToString() ?? "N/A" }
            // });

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = "An error occurred while processing your request.",
            Details = exception.Message // Avoid showing stack trace or sensitive info in production
        };

        var jsonResponse = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(jsonResponse);
    }
}