using Microsoft.EntityFrameworkCore;
using Payments.Management.API.Contexts;
using Payments.Management.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BusinessManagementDatabase")));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

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