using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Tillegal.API;
using Tillegal.API.Common;
using Tillegal.Application;
using Tillegal.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Presentation / API
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddPresentation();

// Application
builder.Services.AddApplication();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

builder.Logging.AddJsonConsole();

// CORS and rate limiting
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
    ForwardedHeadersConfiguration.ConfigureKnownProxies(options, builder.Configuration));

builder.Services.AddHealthChecks()
    .AddCheck<PostgreSqlHealthCheck>("postgresql", tags: ["ready"])
    .AddCheck<RedisHealthCheck>("redis", tags: ["ready"])
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"])
    .AddCheck<SupabaseStorageHealthCheck>("supabase-storage", tags: ["ready"]);

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
OpenTelemetryBuilder openTelemetry = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("tillegal-api"));
openTelemetry.WithMetrics(metrics =>
{
    metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation();
    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out Uri? metricsEndpoint))
    {
        metrics.AddOtlpExporter(options => options.Endpoint = metricsEndpoint);
    }
});
openTelemetry.WithTracing(tracing =>
{
    tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();
    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out Uri? tracesEndpoint))
    {
        tracing.AddOtlpExporter(options => options.Endpoint = tracesEndpoint);
    }
});

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Build Application
WebApplication app = builder.Build();

// Exception Handling
app.UseExceptionHandler();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP Pipeline
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});
app.Run();
