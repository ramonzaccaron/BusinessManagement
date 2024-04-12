using Microsoft.EntityFrameworkCore;
using Payments.Management.API.Domain;

namespace Payments.Management.API.Contexts
{
    // EF Comands:
    // Add-Migration InitialDatabase
    // Update-Database
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }
    }
}
