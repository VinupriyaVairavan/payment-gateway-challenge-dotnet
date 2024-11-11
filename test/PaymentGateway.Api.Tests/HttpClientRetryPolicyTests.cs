using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Moq;
using Moq.Protected;
using PaymentGateway.Models.Requests;
using Polly;
using Xunit;

namespace PaymentGateway.Api.Tests;

public class HttpClientRetryPolicyTestsTest
{
    private IHttpClientFactory _httpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public HttpClientRetryPolicyTestsTest()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    }

    [Fact]
    public async Task Test_RetryPolicy_Should_Retry_On_Failure()
    {
        _mockHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadGateway)) // Simulate a failure
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)) // Simulate a failure
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // Simulate a success on the third attempt
        
        // Register the HttpClient with the retry policy and mock handler
        var services = new ServiceCollection();
        
        services.AddSingleton(_mockHandler.Object);
        services.AddHttpClient("MyApiClient")
            .ConfigurePrimaryHttpMessageHandler(() => _mockHandler.Object)
            .AddResilienceHandler("pipeline-name", handler =>
            {
                handler.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3, // Number of retries
                    Delay = TimeSpan.FromSeconds(2), // Initial delay
                    BackoffType = DelayBackoffType.Exponential
                });
            
                handler.AddTimeout(TimeSpan.FromSeconds(10)); // Timeout duration
            
                handler.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 10, // Minimum number of requests
                    BreakDuration = TimeSpan.FromSeconds(60) // Duration to keep the circuit open
                });
            });

        _httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

        var httpClient = _httpClientFactory.CreateClient("MyApiClient"); // new HttpClient(_mockHandler.Object);

        // Act: Make the HTTP request
        var response = await httpClient.PostAsJsonAsync("http://localhost:8080/payments", new PostPaymentProviderRequest());
        
        // Assert: Check if the response is a success (200 OK)
        Assert.True(response.IsSuccessStatusCode);
        _mockHandler.Protected().Verify(
            "SendAsync", 
            Times.Exactly(3),  // Verify that SendAsync was called exactly 3 times
            ItExpr.IsAny<HttpRequestMessage>(), 
            ItExpr.IsAny<CancellationToken>()
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Test_RetryPolicy_Should_Not_Retry_On_Success()
    {
        // Arrange: Setup the mock to simulate success HTTP request
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // Simulate a success
        
        var services = new ServiceCollection();
        
        services.AddSingleton(_mockHandler.Object);
        services.AddHttpClient("MyApiClient")
            .ConfigurePrimaryHttpMessageHandler(() => _mockHandler.Object)
            .AddResilienceHandler("pipeline-name", handler =>
            {
                handler.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3, // Number of retries
                    Delay = TimeSpan.FromSeconds(2), // Initial delay
                    BackoffType = DelayBackoffType.Exponential
                });
            
                handler.AddTimeout(TimeSpan.FromSeconds(10)); // Timeout duration
            
                handler.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 10, // Minimum number of requests
                    BreakDuration = TimeSpan.FromSeconds(60) // Duration to keep the circuit open
                });
            });

        _httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        
        var httpClient = _httpClientFactory.CreateClient("MyApiClient"); 

        // Act: Make the HTTP request
        var response = await httpClient.PostAsJsonAsync("http://localhost:8080/payments", new PostPaymentProviderRequest());
        
        // Assert: Check if the response is a success (200 OK)
        Assert.True(response.IsSuccessStatusCode);
        _mockHandler.Protected().Verify(
            "SendAsync", 
            Times.Exactly(1),  // Verify that SendAsync was called exactly 1 time
            ItExpr.IsAny<HttpRequestMessage>(), 
            ItExpr.IsAny<CancellationToken>()
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}