using Microsoft.AspNetCore.Mvc;
using Payments.Management.API.Commands;
using Payments.Management.API.Contexts;
using Payments.Management.API.Domain;
using Payments.Management.API.Meters;
using Payments.Management.API.Queries;

namespace Payments.Management.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(AppDbContext dbContext, ILogger<PaymentController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PaymentListQuery>> GetPayments()
        {
            _logger.LogInformation("Selecionando todos os pagamentos.");

            //Utilização de métrica customizada - Counter
            PaymentMetrics.GetRequestCounter.Add(1,
                new("Action", nameof(GetPayments)),
                new("Controller", nameof(PaymentController)));

            return _dbContext.Payments.Select(e =>
                new PaymentListQuery()
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    Role = e.Role,
                    Salary = e.Salary,
                }
            ).ToList();
        }

        [HttpPost]
        public ActionResult AddPayment(AddPaymentCommand command)
        {
            _logger.LogInformation("Iniciando processo de registro de pagamento.");

            var payment = new Payment(command.EmployeeId, command.Role);

            _dbContext.Payments.Add(payment);
            _dbContext.SaveChanges();

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
