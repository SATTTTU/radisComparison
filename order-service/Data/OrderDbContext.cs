using Microsoft.EntityFrameworkCore;
using OrderServiceApp.Domain;

namespace OrderServiceApp.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> opts) : base(opts) { }
        public DbSet<Order> Orders { get; set; } = null!;
    }
}
