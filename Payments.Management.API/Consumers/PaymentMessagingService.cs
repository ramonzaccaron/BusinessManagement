using Business.Management.Contracts;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Payments.Management.API.Contexts;
using Payments.Management.API.Controllers;
using Payments.Management.API.Domain;

namespace Payments.Management.API.Consumers
{
    public class PaymentMessagingService : IConsumer<EmployeeAddedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentController> _logger;
        private readonly IDistributedCache _distributedCache;

        public PaymentMessagingService(AppDbContext dbContext, ILogger<PaymentController> logger, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _logger = logger;
            _distributedCache = distributedCache;
        }   
        public Task Consume(ConsumeContext<EmployeeAddedEvent> context)
        {
            _logger.LogInformation("Iniciando processo de registro de pagamento via mensageria.");

            // Para simularmos um longo processamento do evento - 30s - Visualização de processamento
            //Thread.Sleep(30000);

            var payment = new Payment(context.Message.EmployeeId, context.Message.Role);

            _dbContext.Payments.Add(payment);
            _dbContext.SaveChanges();

            _logger.LogInformation("Removendo dados de cache para a próxima consulta buscar do bando e atualizar o Redis.");
            _distributedCache.RemoveAsync("Payment");

            _logger.LogInformation("Finalizando processo de registro de pagamento via mensageria.");

            return Task.CompletedTask;
        }
    }
}
