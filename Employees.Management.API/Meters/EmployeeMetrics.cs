using System.Diagnostics.Metrics;

namespace Employees.Management.API.Meters
{
    //Employee meters
    public class EmployeeMetrics
    {
        private const string serviceName = "EmployeeManagementService";

        public static string ServiceName => serviceName;

        public static Meter Meter { get; set; } = new(ServiceName);

        public static Counter<long> GetRequestCounter { get; set; } = 
            Meter.CreateCounter<long>(name: "custom.get.employee.request_counter", unit: "Employee", description: "Quantidade de requisições de buscas de funcionários");

        public static Counter<long> NewEmployeesCounter { get; set; } = 
            Meter.CreateCounter<long>(name: "custom.new.employees_counter", unit: "Employee", description: "Quantidade de funcionários cadastrados");

        public static Histogram<double> NewEmployeePaymentHistogram { get; set; } =
            Meter.CreateHistogram<double>(name: "custom.http.payment.server.duration", description: "Mede a duração da chamada HTTP para a API de Pagamento", unit: "ms");
    }
}
