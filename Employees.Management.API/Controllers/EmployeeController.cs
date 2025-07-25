using Business.Management.Contracts;
using Employees.Management.API.Commands;
using Employees.Management.API.Contexts;
using Employees.Management.API.Domain;
using Employees.Management.API.Infra;
using Employees.Management.API.Meters;
using Employees.Management.API.Queries;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

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
        private readonly IDistributedCache _distributedCache;

        public EmployeeController(IHttpClientFactory clientFactory, AppDbContext dbContext, ILogger<EmployeeController> logger, IBus bus, IDistributedCache distributedCache)
        {
            _clientFactory = clientFactory;
            _dbContext = dbContext;
            _logger = logger;
            _bus = bus;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EmployeeListQuery>> GetEmployees()
        {
            _logger.LogInformation("Selecionando todos os funcionários.");

            //Utilização de métrica customizada - Counter
            EmployeeMetrics.GetRequestCounter.Add(1,
                new("Action", nameof(GetEmployees)),
                new("Controller", nameof(EmployeeController)));


            var employees = _distributedCache.GetAsync("Employee");

            if (employees.Result != null)
            {
                _logger.LogInformation("Dados encontrados em cache, retornando dados do Redis.");

                var cachedDataString = Encoding.UTF8.GetString(employees.Result);
                var employeeList = JsonConvert.DeserializeObject<List<EmployeeListQuery>>(cachedDataString);

                return employeeList;
            }

            _logger.LogInformation("Dados não encontrados em cache, buscando dados do Banco.");

            var dbEmployees = _dbContext.Employees.Select(e =>
                new EmployeeListQuery()
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    BirthDate = e.BirthDate,
                    Role = e.Role,
                }
            ).ToList();

            _logger.LogInformation("Inserindo dados em cache para próximas consultas.");

            string serializedEmployeeList = JsonConvert.SerializeObject(dbEmployees);
            _distributedCache.SetAsync("Employee", Encoding.UTF8.GetBytes(serializedEmployeeList));

            _logger.LogInformation("Retornando dados do Banco.");

            return dbEmployees;
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

            _logger.LogInformation("Removendo dados de cache para a próxima consulta buscar do bando e atualizar o Redis.");
            await _distributedCache.RemoveAsync("Employee");

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
