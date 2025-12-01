using Microsoft.EntityFrameworkCore;
using order_service.Data;
using order_service.Domain;

namespace order_service.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderAsync(int id);
    Task<Order?> UpdateOrderStatusAsync(int id, string status);
}

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetOrderAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order?> UpdateOrderStatusAsync(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return null;

        order.Status = status;
        await _context.SaveChangesAsync();
        return order;
    }
}
