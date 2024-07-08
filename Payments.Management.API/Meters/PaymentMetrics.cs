using System.Diagnostics.Metrics;

namespace Payments.Management.API.Meters
{
    //Payment meters
    public class PaymentMetrics
    {
        private const string serviceName = "PaymentManagementService";

        public static string ServiceName => serviceName;

        public static Meter Meter { get; set; } = new(ServiceName);

        public static Counter<long> GetRequestCounter { get; set; } =
            Meter.CreateCounter<long>(name: "custom.get.payment.request_counter", unit: "Payment", description: "Quantidade de requisições de buscas de pagamentos");

        public static Counter<long> NewPaymentCounter { get; set; } =
            Meter.CreateCounter<long>(name: "custom.new.payment_counter", unit: "Payment", description: "Quantidade de pagamentos cadastrados");
    }
}
