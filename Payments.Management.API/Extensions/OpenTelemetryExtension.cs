using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Payments.Management.API.Extensions
{
    public static class OpenTelemetryExtension
    {
        public static void AddCustomOpenTelemetry(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "ServiceName").Value))
                        .AddSource(configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "SourceName").Value)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .SetSampler(new AlwaysOnSampler())
                        .AddNpgsql()
                        .SetErrorStatusOnException()
                        .AddOtlpExporter(opts =>
                        {
                            opts.Endpoint = new Uri(configuration.GetSection("OpenTelemetrySettings").GetChildren().FirstOrDefault(c => c.Key == "OtlExporterEndpoint").Value);
                        });
                });
        }
    }
}
