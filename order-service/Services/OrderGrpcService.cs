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
        private readonly shared.Messaging.RabbitMqPublisher _publisher;
        private readonly ILogger<OrderGrpcService> _logger;

        public OrderGrpcService(IOrderRepository repo, shared.Messaging.RabbitMqPublisher publisher, ILogger<OrderGrpcService> logger)
        {
            _repo = repo;
            _publisher = publisher;
            _logger = logger;
        }

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating Order for User {UserId} with Amount {Amount}", request.UserId, request.Amount);

            var order = new Order
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = OrderStatus.Pending
            };

            var created = await _repo.CreateAsync(order);
            _logger.LogInformation("Order Created: {OrderId}", created.Id);

            // Publish OrderCreatedEvent
            var orderEvent = new shared.Events.OrderCreatedEvent
            {
                OrderId = created.Id,
                Amount = created.Amount
            };

            _logger.LogInformation("Publishing OrderCreatedEvent for Order {OrderId}", created.Id);
            await _publisher.PublishAsync("order_exchange", "order.created", orderEvent);

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
