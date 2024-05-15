using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Employees.Management.API.Extensions
{
    public static class OpenTelemetryExtension
    {
        public static void AddCustomOpenTelemetry(this IServiceCollection services, ConfigurationManager configuration)
        {
            var serviceName = configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "ServiceName").Value;
            var otelExporterEndpoint = configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "OtlExporterEndpoint").Value;

                services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(x => x.SetDbStatementForText = true)
                        .AddNpgsql()
                        .AddOtlpExporter(opts =>
                        {
                            opts.Endpoint = new Uri(otelExporterEndpoint);
                        });
                });
        }
    }
}
