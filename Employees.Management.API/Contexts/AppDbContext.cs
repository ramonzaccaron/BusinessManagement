using Employees.Management.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace Employees.Management.API.Contexts
{
    // EF Comands:
    // Add-Migration InitialDatabase
    // Update-Database
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
