using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payments.Management.API.Contexts;

namespace Payments.Management.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static void MigrateDb(this WebApplication application)
        {
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var migrator = db.Database.GetService<IMigrator>();
                migrator.Migrate();
            }
        }
    }
}
