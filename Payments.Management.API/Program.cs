using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payments.Management.API.Consumers;
using Payments.Management.API.Contexts;
using Payments.Management.API.Extensions;
using Payments.Management.API.Infra;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BusinessManagementDatabase")));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddCustomOpenTelemetry(builder.Configuration);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentMessagingService>();

    x.UsingRabbitMq((context, config) =>
    {
        config.Host(new Uri(RabbitMqConsts.RabbitMqRootUri), h =>
        {
            h.Username(RabbitMqConsts.UserName);
            h.Password(RabbitMqConsts.Password);
        });
        config.ReceiveEndpoint("payment", e =>
        {
            e.Consumer<PaymentMessagingService>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        opt.RoutePrefix = string.Empty;
    });
}

app.MigrateDb();

app.Run();