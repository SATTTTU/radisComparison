using AspireSample.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireSample.ApiService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ChatUser> Users { get; set; }
}
