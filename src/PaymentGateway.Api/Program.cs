using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using PaymentGateway.Api.ErrorHandler;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Settings;
using PaymentGateway.Api;
using PaymentGateway.Api.AutomapperProfiles;
using PaymentGateway.Api.Services.Validators;
using PaymentGateway.Api.Validators;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Log.Logger = new LoggerConfiguration()
builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        // .WriteTo.ApplicationInsights(new TelemetryConfiguration { InstrumentationKey = "INSTRUMENTATION_KEY" },
        //     TelemetryConverter.Traces)
        .WriteTo.File("Logs/log-Dev.txt", rollingInterval: RollingInterval.Day) // Log to a file with daily rolling
);

builder.Services.AddAutoMapper(typeof(Program)); 
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
builder.Services.AddSingleton<IPaymentIdProvider, PaymentIdProvider>();
builder.Services.Configure<PaymentProviderSetting>(builder.Configuration.GetSection("PaymentProvider"));
builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddTransient(typeof(CardNumberMaskResolver<,,>));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(Constants.MY_API_CLIENT, (serviceProvider, client)  =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PaymentProviderSetting>>().Value;
    client.BaseAddress = new Uri(options.ApiUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeOutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
    .AddResilienceHandler("pipeline-resilience", handler =>
{
    //All the below hard coded values can be set/read from json settings file using IOptions, as above
    //To Keep it simple, I have Hard coded the values here
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

builder.Services.AddHttpClient<PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();

app.UseAuthorization();
    
app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseExceptionHandler(Constants.ERROR_HANDLE);
app.UseStatusCodePagesWithReExecute(Constants.ERROR_NOT_FOUND);

app.MapControllers();
app.Run();
