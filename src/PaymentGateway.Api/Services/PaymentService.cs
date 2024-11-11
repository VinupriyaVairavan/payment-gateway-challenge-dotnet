using System.Diagnostics;
using AutoMapper;
using PaymentGateway.Api.Entities;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services.Validators;
using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentService(
    ILoggerService<PaymentService> logger, 
    IHttpClientFactory httpClientFactory, 
    IPaymentIdProvider paymentIdProvider,
    IPaymentRepository paymentRepository,
    IMapper mapper,
    IValidationService validationService) 
    : IPaymentService
{
    public async Task<PostPaymentResponse?> ProcessPaymentAsync(PostPaymentRequest paymentRequest)
    {
        //Log every payment request
        logger.LogInformation("Processing payment for request: {PaymentRequest}", paymentRequest);

        var paymentResponse = mapper.Map<PostPaymentResponse>(paymentRequest);
        var paymentProviderRequest = mapper.Map<PostPaymentProviderRequest>(paymentRequest);
        
        // Validate incoming request
        if (!ValidatePaymentRequest(paymentRequest, paymentResponse))
            return paymentResponse;

        if (paymentRequest.Id != Guid.Empty) //Idempotent request processing
        {
            var getResponse = await GetPaymentAsync(paymentRequest.Id);

            if (getResponse == null || getResponse.Status == PaymentStatus.Authorized.ToString())
                return mapper.Map<PostPaymentResponse>(getResponse);
        }
        else
        {
            // Generate a payment id for the request
            paymentProviderRequest.Id = paymentIdProvider.Generate(paymentRequest);
        }

        //Invoke Payment processor
        var client = httpClientFactory.CreateClient(Constants.MY_API_CLIENT);
        var response =
            await client.PostAsJsonAsync(Constants.PAYMENTS_ENDPOINT, paymentProviderRequest);

        var content = await response.Content.ReadFromJsonAsync<PostPaymentProviderResponse>();

        paymentResponse.Id = paymentProviderRequest.Id;
        paymentResponse.Status = content?.Authorized == true
            ? PaymentStatus.Authorized.ToString()
            : PaymentStatus.Declined.ToString();

        paymentResponse.AuthorizationCode = content?.Authorized == true
            ? content.AuthorizationCode
            : null;

        //Store the payment request with generated payment id
        var payment = mapper.Map<Payment>(paymentResponse);
        payment.Id = paymentProviderRequest.Id;
        await paymentRepository.AddAsync(payment);
        return paymentResponse;
    }

    public async Task<GetPaymentResponse?> GetPaymentAsync(Guid paymentId)
    {
        GetPaymentResponse response = new();
        try
        {
            response = mapper.Map<GetPaymentResponse>(await paymentRepository.GetAsync(paymentId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex, paymentId);
        }
        
        return response;
    }

    private bool ValidatePaymentRequest(PostPaymentRequest paymentRequest, PostPaymentResponse paymentResponse)
    {
        var validationErrors = validationService.ValidatePaymentRequest(paymentRequest);

        if (validationErrors.Any())
        {
            paymentResponse.Errors = validationErrors;
            paymentResponse.Status = PaymentStatus.Rejected.ToString();
            return false;
        }
        return true;
    }
}