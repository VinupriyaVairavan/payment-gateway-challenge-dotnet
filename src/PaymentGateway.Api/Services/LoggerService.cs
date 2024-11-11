using System.Diagnostics;
using System.Text.Json;

namespace PaymentGateway.Api.Services;

public class LoggerService<T>(ILogger<T> logger) : ILoggerService<T>
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private string SerializeModel(object? model)
    {
        return model == null ? string.Empty : JsonSerializer.Serialize(model, _jsonOptions);
    }
    public void LogInformation(string message, object? model, params object[] args)
    {
        message += GenerateActivityTrace();
        if (model != null)
        {
            message += $"Model: {model}";
            logger.LogInformation(message, args.Append(SerializeModel(model)).ToArray());
        }
        else
        {
            logger.LogInformation(message, args);
        }
    }

    public void LogWarning(string message, object? model, params object[] args)
    {
        message += GenerateActivityTrace();
        if (model != null)
        {
            message += " Model: {Model}";
            logger.LogWarning(message, args.Append(SerializeModel(model)).ToArray());
        }
        else
        {
            logger.LogWarning(message, args);
        }
    }

    public void LogError(string message, Exception? exception, object? model, params object[] args)
    {
        message += GenerateActivityTrace();
        if (model != null)
        {
            message += " Model: {Model}";
            if (exception != null)
            {
                logger.LogError(exception, message, args.Append(SerializeModel(model)).ToArray());
            }
            else
            {
                logger.LogError(message, args.Append(SerializeModel(model)).ToArray());
            }
        }
        else
        {
            if (exception != null)
            {
                logger.LogError(exception, message, args);
            }
            else
            {
                logger.LogError(message, args);
            }
        }
    }

    public void LogDebug(string message, object? model, params object[] args)
    {
        message += GenerateActivityTrace();
        if (model != null)
        {
            message += " Model: {Model}";
            logger.LogDebug(message, args.Append(SerializeModel(model)).ToArray());
        }
        else
        {
            logger.LogDebug(message, args);
        }
    }

    public void LogCritical(string message, Exception? exception, object? model , params object[] args)
    {
        message += GenerateActivityTrace();
        if (model != null)
        {
            message += " Model: {Model}";
            if (exception != null)
            {
                logger.LogCritical(exception, message, args.Append(SerializeModel(model)).ToArray());
            }
            else
            {
                logger.LogCritical(message, args.Append(SerializeModel(model)).ToArray());
            }
        }
        else
        {
            if (exception != null)
            {
                logger.LogCritical(exception, message, args);
            }
            else
            {
                logger.LogCritical(message, args);
            }
        }
    }

    private string GenerateActivityTrace()
    {
        var activity = Activity.Current;
            
        return $"Trace ID: {activity?.TraceId.ToString() ?? "N/A"}, " +
                   $"Span ID: {activity?.SpanId.ToString() ?? "N/A"}";
    }
}