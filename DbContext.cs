using Microsoft.EntityFrameworkCore;
using ConsoleApplication;

namespace ConsoleApplication
{
    public class AppDbContext : DbContext
    {
        public DbSet<BankAccount> BankAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=info;Username=postgres;Password=admin;");
        }

    }
}
