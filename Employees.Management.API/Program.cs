using Employees.Management.API.Contexts;
using Employees.Management.API.Extensions;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddCustomOpenTelemetry(builder.Configuration);

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