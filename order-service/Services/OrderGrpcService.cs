using Grpc.Core;
using OrderProto;
using OrderServiceApp.Domain;
using OrderServiceApp.Repositories;
using System.Threading.Tasks;
namespace OrderServiceApp.Services
{
    public class OrderGrpcService : OrderService.OrderServiceBase
    {
        private readonly IOrderRepository _repo;
        public OrderGrpcService(IOrderRepository repo) => _repo = repo;

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var order = new Order
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = OrderStatus.Pending
            };

            var created = await _repo.CreateAsync(order);

            return new CreateOrderResponse
            {
                OrderId = created.Id,
                Amount = created.Amount,
                Currency = created.Currency,
                Status = created.Status.ToString()
            };
        }

        public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

            return new GetOrderResponse
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Amount = order.Amount,
                Currency = order.Currency,
                Status = order.Status.ToString()
            };
        }

        public override async Task<UpdateOrderStatusResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
        {
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return new UpdateOrderStatusResponse { Ok = false };

            if (System.Enum.TryParse<OrderStatus>(request.Status, out var s))
                order.Status = s;

            await _repo.UpdateAsync(order);

            return new UpdateOrderStatusResponse { Ok = true };
        }
    }
}
