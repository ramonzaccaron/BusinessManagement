using MassTransit.Logging;
using MassTransit.Monitoring;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Payments.Management.API.Meters;

namespace Payments.Management.API.Extensions
{
    public static class OpenTelemetryExtension
    {
        public static void AddCustomOpenTelemetry(this IServiceCollection services, ConfigurationManager configuration)
        {
            var serviceName = configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "ServiceName").Value;
            var otelExporterEndpoint = configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "OtlExporterEndpoint").Value;

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName, string.Empty, "1.0", true, Environment.MachineName))
                .UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(otelExporterEndpoint))
                .WithTracing(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(x => x.SetDbStatementForText = true)
                        .AddNpgsql()
                        .AddSource(DiagnosticHeaders.DefaultListenerName); // MassTransit ActivitySource
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddMeter(PaymentMetrics.Meter.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddMeter(InstrumentationOptions.MeterName); // MassTransit Meter
                });
        }
    }
}
