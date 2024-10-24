using Business.Management.Contracts;
using Employees.Management.API.Commands;
using Employees.Management.API.Contexts;
using Employees.Management.API.Domain;
using Employees.Management.API.Infra;
using Employees.Management.API.Meters;
using Employees.Management.API.Queries;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Employees.Management.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<EmployeeController> _logger;
        readonly IBus _bus;

        public EmployeeController(IHttpClientFactory clientFactory, AppDbContext dbContext, ILogger<EmployeeController> logger, IBus bus)
        {
            _clientFactory = clientFactory;
            _dbContext = dbContext;
            _logger = logger;
            _bus = bus;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EmployeeListQuery>> GetEmployees()
        {
            _logger.LogInformation("Selecionando todos os funcionários.");

            //Utilização de métrica customizada - Counter
            EmployeeMetrics.GetRequestCounter.Add(1,
                new("Action", nameof(GetEmployees)),
                new("Controller", nameof(EmployeeController)));

            return _dbContext.Employees.Select(e =>
                new EmployeeListQuery()
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    BirthDate = e.BirthDate,
                    Role = e.Role,
                }
            ).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> AddEmployee(AddEmployeeCommand command)
        {
            _logger.LogInformation("Iniciando processo de registro de funcionário.");

            var employee = new Employee()
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                BirthDate = command.BirthDate,
                Role = command.Role
            };

            _dbContext.Employees.Add(employee);
            _dbContext.SaveChanges();

            //Utilização de métrica customizada - Counter
            EmployeeMetrics.NewEmployeesCounter.Add(1,
                new("Action", nameof(AddEmployee)),
                new("Controller", nameof(EmployeeController)),
                new("EmployeeId", employee.Id));

            var paymentCommand = new AddPaymentCommand()
            {
                EmployeeId = employee.Id,
                Role = command.Role
            };

            // Para testar o monitoramento tanto de requisições HTTP quanto via Mensageria
            // será enviado para a API de pagamento o cadastro de um novo funcionário das duas formas.

            _logger.LogInformation("Iniciando processo de envio de Funcionário para Pagamento via mensageria.");

            var newEvent = new EmployeeAddedEvent()
            {
                EmployeeId = employee.Id,
                Role = employee.Role
            };

            var url = new Uri(RabbitMqConsts.RabbitMqUri);

            var endpoint = await _bus.GetSendEndpoint(url);
            await endpoint.Send(newEvent);

            _logger.LogInformation("Finalizando processo de envio de Funcionário para Pagamento via mensageria.");

            _logger.LogInformation("Iniciando processo de envio de Funcionário para Pagamento.");

            using (var client = _clientFactory.CreateClient("PaymentAPI"))
            {
                HttpRequestMessage message = new()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(paymentCommand), System.Text.Encoding.UTF8, "application/json")
                };

                var stopwatch = Stopwatch.StartNew();

                HttpResponseMessage response = client.Send(message);

                //Utilização de métrica customizada - Histogram
                EmployeeMetrics.NewEmployeePaymentHistogram.Record(stopwatch.ElapsedMilliseconds,
                    tag: KeyValuePair.Create<string, object?>("Host", client.BaseAddress));

                await response.Content.ReadAsStringAsync();
            }

            _logger.LogInformation("Finalizando processo de envio de Funcionário para Pagamento.");

            _logger.LogInformation("Finalizando processo de registro de funcionário.");

            return Ok();
        }
    }
}
