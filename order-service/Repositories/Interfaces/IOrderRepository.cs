using OrderServiceApp.Domain;
using System.Threading.Tasks;

namespace OrderServiceApp.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task UpdateAsync(Order order);
    }
}
