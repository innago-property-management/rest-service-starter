namespace Innago.Shared.ReplaceMe;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Prometheus;

using RestSharp;

using Serilog;
using Serilog.Events;

internal static class ProgramConfiguration
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOpenApi();

        services.AddOpenTelemetry().WithTracing(ConfigureTracing);

        services.AddSerilog();

        services.AddHealthChecks().ForwardToPrometheus();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddHttpContextAccessor();

        // ReSharper disable once SeparateLocalFunctionsWithJumpStatement
        void ConfigureTracing(TracerProviderBuilder providerBuilder)
        {
            string serviceName = configuration["opentelemetry:serviceName"] ??
                                 throw new InvalidOperationException("missing service name: set environment variable opentelemetry__serviceName");

            providerBuilder.AddSource(serviceName);
            providerBuilder.ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName));
            providerBuilder.AddHttpClientInstrumentation();
            providerBuilder.AddAspNetCoreInstrumentation();

            if (environment.IsDevelopment())
            {
                providerBuilder.AddConsoleExporter();
            }

            providerBuilder.AddOtlpExporter(options =>
            {
                bool isGoodUri = Uri.TryCreate(configuration["opentelemetry:endpoint"], UriKind.Absolute, out Uri? uri);

                if (!isGoodUri)
                {
                    return;
                }

                options.Endpoint = uri!;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        }
    }

    public static void ConfigureApplicationBuilder(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseHttpMetrics();
    }

    public static void ConfigureRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapOpenApi("/openapi.json").CacheOutput();
        builder.MapHealthChecks("/healthz/live", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("live") });
        builder.MapHealthChecks("/healthz/ready", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("ready") });
        builder.MapMetrics("/metricsz");

        builder.MapPost("/placeholder", Handlers.Placeholder.Stub).WithTags("__REPLACE_ME__");
    }

    internal static LoggerConfiguration SetLogLevelsFromConfig(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        IConfigurationSection minimumLevelSection = configuration.GetSection("Serilog:MinimumLevel");

        loggerConfiguration.MinimumLevel.Is(minimumLevelSection["default"].ToLogEventLevel());

        IConfigurationSection overrideSection = minimumLevelSection.GetSection("Override");

        foreach (IConfigurationSection overrideEntry in overrideSection.GetChildren())
        {
            loggerConfiguration.MinimumLevel.Override(overrideEntry.Key, overrideEntry.Value.ToLogEventLevel());
        }

        return loggerConfiguration;
    }

    private static LogEventLevel ToLogEventLevel(this string? logLevel)
    {
        return Enum.TryParse(logLevel, true, out LogEventLevel logEventLevel) ? logEventLevel : LogEventLevel.Error;
    }
}