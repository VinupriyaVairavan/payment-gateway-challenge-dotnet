using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentGateway.Api.Entities;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Validators;
using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

using Xunit;

public class PaymentServiceTests
{
    private readonly Mock<ILoggerService<PaymentService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IPaymentIdProvider> _paymentIdProviderMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidationService> _validationServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _loggerMock = new Mock<ILoggerService<PaymentService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _paymentIdProviderMock = new Mock<IPaymentIdProvider>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _mapperMock = new Mock<IMapper>();
        _validationServiceMock = new Mock<IValidationService>();

        _paymentService = new PaymentService(
            _loggerMock.Object,
            _httpClientFactoryMock.Object,
            _paymentIdProviderMock.Object,
            _paymentRepositoryMock.Object,
            _mapperMock.Object,
            _validationServiceMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidRequest_ReturnsAuthorizedResponse()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest();
        var paymentResponse = new PostPaymentResponse{Status = PaymentStatus.Authorized.ToString(), AuthorizationCode = "12345"};
        var paymentProviderResponse = new PostPaymentProviderResponse { Authorized = true, AuthorizationCode = "12345" };
        var payment = new Payment();

        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(It.IsAny<PostPaymentRequest>())).Returns(paymentResponse);
        _mapperMock.Setup(m => m.Map<PostPaymentProviderRequest>(It.IsAny<PostPaymentRequest>())).Returns(new PostPaymentProviderRequest());
        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(It.IsAny<PostPaymentProviderResponse>())).Returns(paymentResponse);
        _mapperMock.Setup(m => m.Map<Payment>(It.IsAny<PostPaymentResponse>())).Returns(payment);
        _paymentIdProviderMock.Setup(p => p.Generate(It.IsAny<PostPaymentRequest>())).Returns(Guid.NewGuid());
        _validationServiceMock.Setup(v => v.ValidatePaymentRequest(It.IsAny<PostPaymentRequest>())).Returns(new List<string>());
        
        var httpClient = new HttpClient(new FakeHttpMessageHandler(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(paymentProviderResponse)
        }))
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(h => h.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.Equal(PaymentStatus.Authorized.ToString(), result.Status);
        Assert.Equal(paymentProviderResponse.AuthorizationCode, result.AuthorizationCode);
    }

    [Fact]
    public async Task ProcessPaymentAsync_InvalidRequest_ReturnsRejectedResponse()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest();
        var paymentResponse = new PostPaymentResponse();
        var validationErrors = new List<string> { "Invalid request" };

        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(It.IsAny<PostPaymentRequest>())).Returns(paymentResponse);
        _validationServiceMock.Setup(v => v.ValidatePaymentRequest(It.IsAny<PostPaymentRequest>())).Returns(validationErrors);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.Equal(PaymentStatus.Rejected.ToString(), result.Status);
        Assert.Equal(validationErrors, result.Errors);
    }

    [Fact]
    public async Task GetPaymentAsync_ValidId_ReturnsPaymentResponse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment();
        var paymentResponse = new GetPaymentResponse();

        _paymentRepositoryMock.Setup(p => p.GetAsync(It.IsAny<Guid>())).ReturnsAsync(payment);
        _mapperMock.Setup(m => m.Map<GetPaymentResponse>(It.IsAny<Payment>())).Returns(paymentResponse);

        // Act
        var result = await _paymentService.GetPaymentAsync(paymentId);

        // Assert
        Assert.Equal(paymentResponse, result);
    }

    [Fact]
    public async Task GetPaymentAsync_InvalidId_ReturnsEmptyResponse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _paymentRepositoryMock.Setup(p => p.GetAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Payment not found"));

        // Act
        var result = await _paymentService.GetPaymentAsync(paymentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, Guid.Empty);
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}