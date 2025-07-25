using Employees.Management.API.Contexts;
using Employees.Management.API.Extensions;
using Employees.Management.API.Infra;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BusinessManagementDatabase")));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("PaymentAPI", httpClient =>
{
    httpClient.BaseAddress = new Uri(new Uri(builder.Configuration.GetValue<string>("PaymentUrl")), "/Payment");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((cfg, context) =>
    {
        context.Host(new Uri(RabbitMqConsts.RabbitMqRootUri), h =>
        {
            h.Username(RabbitMqConsts.UserName);
            h.Password(RabbitMqConsts.Password);
        });
    });
});

IConnectionMultiplexer redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisConsts.RedisRootUri);

builder.Services.AddSingleton(redisConnectionMultiplexer);
builder.Services.AddStackExchangeRedisCache(options => options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnectionMultiplexer));

builder.Services.AddCustomOpenTelemetry(builder.Configuration, redisConnectionMultiplexer);

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