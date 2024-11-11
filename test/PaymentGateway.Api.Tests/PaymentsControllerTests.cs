using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> mockPaymentService;
    private readonly PaymentsController controller;

    public PaymentsControllerTests()
    {
        mockPaymentService = new Mock<IPaymentService>();
        controller = new PaymentsController(mockPaymentService.Object);
    }

    [Fact]
    public async Task PostPaymentAsync_ShouldReturnOkResult_WithPaymentResponse()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest { Id = Guid.NewGuid() };
        var expectedResponse = new PostPaymentResponse {Status = PaymentStatus.Authorized.ToString()};

        mockPaymentService
            .Setup(service => service.ProcessPaymentAsync(paymentRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await controller.PostPaymentAsync(paymentRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>(); 
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().Be(expectedResponse);
    }
    
    [Fact]
    public async Task GetPaymentAsync_ShouldReturnOkResult_WithPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var expectedPayment = new GetPaymentResponse ();

        mockPaymentService
            .Setup(service => service.GetPaymentAsync(paymentId))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await controller.GetPaymentAsync(paymentId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();  
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().Be(expectedPayment); 
    }
    
    [Fact]
    public async Task GetPaymentAsync_ShouldReturnNotFoundResult_WhenPaymentIdDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var expectedPayment = new GetPaymentResponse();

        mockPaymentService
            .Setup(service => service.GetPaymentAsync(paymentId))
            .ReturnsAsync((GetPaymentResponse?)null);

        // Act
        var result = await controller.GetPaymentAsync(paymentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>(); 
    }

    [Fact]
    public async Task PostPaymentAsync_ShouldReturnBadRequestResult_WhenPaymentIsRejectedOrDeclined()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest { Id = Guid.NewGuid() };
        var expectedResponse = new PostPaymentResponse {Status = PaymentStatus.Rejected.ToString()};

        mockPaymentService
            .Setup(service => service.ProcessPaymentAsync(paymentRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await controller.PostPaymentAsync(paymentRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>(); 
        var badResult = result.Result as BadRequestObjectResult;
        badResult.Value.Should().Be(expectedResponse);
    }
    
    [Fact]
    public async Task PostPaymentAsync_ShouldReturnNotFoundRequestResult_WhenPaymentIdNotRecognized()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest { Id = Guid.NewGuid() };

        mockPaymentService
            .Setup(service => service.ProcessPaymentAsync(paymentRequest))
            .ReturnsAsync((PostPaymentResponse?)null);

        // Act
        var result = await controller.PostPaymentAsync(paymentRequest);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}