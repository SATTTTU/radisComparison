using Grpc.Core;
using order_service.Protos;
using order_service.Repositories;
using order_service.Domain;

namespace order_service.Services;

public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(IOrderRepository repository, ILogger<OrderGrpcService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var order = new Order
        {
            Amount = (decimal)request.Amount,
            Status = "Created"
        };

        var createdOrder = await _repository.CreateOrderAsync(order);

        return new OrderResponse
        {
            Id = createdOrder.Id,
            Amount = (double)createdOrder.Amount,
            Status = createdOrder.Status
        };
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = await _repository.GetOrderAsync(request.Id);

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"));
        }

        return new OrderResponse
        {
            Id = order.Id,
            Amount = (double)order.Amount,
            Status = order.Status
        };
    }

    public override async Task<OrderResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
    {
        var updatedOrder = await _repository.UpdateOrderStatusAsync(request.Id, request.Status);

        if (updatedOrder == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"));
        }

        return new OrderResponse
        {
            Id = updatedOrder.Id,
            Amount = (double)updatedOrder.Amount,
            Status = updatedOrder.Status
        };
    }
}
