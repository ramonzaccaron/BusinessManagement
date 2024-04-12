using Microsoft.AspNetCore.Mvc;
using Payments.Management.API.Commands;
using Payments.Management.API.Contexts;
using Payments.Management.API.Domain;
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

            _logger.LogInformation("Finalizando processo de registro de pagamento.");

            return Ok();
        }
    }
}
