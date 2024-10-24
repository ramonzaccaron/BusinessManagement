using Business.Management.Contracts;
using MassTransit;
using Payments.Management.API.Contexts;
using Payments.Management.API.Controllers;
using Payments.Management.API.Domain;

namespace Payments.Management.API.Consumers
{
    public class PaymentMessagingService : IConsumer<EmployeeAddedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentController> _logger;

        public PaymentMessagingService(AppDbContext dbContext, ILogger<PaymentController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public Task Consume(ConsumeContext<EmployeeAddedEvent> context)
        {
            _logger.LogInformation("Iniciando processo de registro de pagamento via mensageria.");

            // Para simularmos um longo processamento do evento - 30s - Visualização de processamento
            //Thread.Sleep(30000);

            var payment = new Payment(context.Message.EmployeeId, context.Message.Role);

            _dbContext.Payments.Add(payment);
            _dbContext.SaveChanges();

            _logger.LogInformation("Finalizando processo de registro de pagamento via mensageria.");

            return Task.CompletedTask;
        }
    }
}
