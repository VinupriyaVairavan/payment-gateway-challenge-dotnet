using Microsoft.Extensions.Logging;

namespace PaymentGateway.Api.Services;

public interface ILoggerService<T>
{
    void LogInformation(string message, object? model = null, params object[] args);
    void LogWarning(string message, object? model = null, params object[] args);
    void LogError(string message, Exception? exception = null, object? model = null, params object[] args);
    void LogDebug(string message, object? model = null, params object[] args);
    void LogCritical(string message, Exception? exception = null, object? model = null, params object[] args);
}