using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Payments.Management.API.Commands;
using Payments.Management.API.Contexts;
using Payments.Management.API.Domain;
using Payments.Management.API.Meters;
using Payments.Management.API.Queries;
using System.Text;

namespace Payments.Management.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentController> _logger;
        private readonly IDistributedCache _distributedCache;

        public PaymentController(AppDbContext dbContext, ILogger<PaymentController> logger, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PaymentListQuery>> GetPayments()
        {
            _logger.LogInformation("Selecionando todos os pagamentos.");

            //Utilização de métrica customizada - Counter
            PaymentMetrics.GetRequestCounter.Add(1,
                new("Action", nameof(GetPayments)),
                new("Controller", nameof(PaymentController)));


            var payments = _distributedCache.GetAsync("Payment");

            if (payments.Result != null)
            {
                _logger.LogInformation("Dados encontrados em cache, retornando dados do Redis.");

                var cachedDataString = Encoding.UTF8.GetString(payments.Result);
                var paymentList = JsonConvert.DeserializeObject<List<PaymentListQuery>>(cachedDataString);

                return paymentList;
            }

            _logger.LogInformation("Dados não encontrados em cache, buscando dados do Banco.");

            var dbPayments = _dbContext.Payments.Select(e =>
                new PaymentListQuery()
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    Role = e.Role,
                    Salary = e.Salary,
                }
            ).ToList();

            _logger.LogInformation("Inserindo dados em cache para próximas consultas.");

            string serializedPaymentList = JsonConvert.SerializeObject(dbPayments);
            _distributedCache.SetAsync("Payment", Encoding.UTF8.GetBytes(serializedPaymentList));

            _logger.LogInformation("Retornando dados do Banco.");

            return dbPayments;
        }

        [HttpPost]
        public ActionResult AddPayment(AddPaymentCommand command)
        {
            _logger.LogInformation("Iniciando processo de registro de pagamento.");

            var payment = new Payment(command.EmployeeId, command.Role);

            _dbContext.Payments.Add(payment);
            _dbContext.SaveChanges();

            _logger.LogInformation("Removendo dados de cache para a próxima consulta buscar do bando e atualizar o Redis.");
            _distributedCache.RemoveAsync("Payment");

            //Utilização de métrica customizada - Counter
            PaymentMetrics.NewPaymentCounter.Add(1,
                new("Action", nameof(AddPayment)),
                new("Controller", nameof(PaymentController)),
                new("Role", payment.Role));

            _logger.LogInformation("Finalizando processo de registro de pagamento.");

            return Ok();
        }
    }
}
