using Microsoft.EntityFrameworkCore;
using OrderServiceApp.Data;
using OrderServiceApp.Domain;
using System.Threading.Tasks;
using OrderServiceApp.Repositories;

namespace OrderServiceApp.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _db;
        public OrderRepository(OrderDbContext db) => _db = db;

        public async Task<Order> CreateAsync(Order order)
        {
            var ent = (await _db.Orders.AddAsync(order)).Entity;
            await _db.SaveChangesAsync();
            return ent;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }
    }
}
