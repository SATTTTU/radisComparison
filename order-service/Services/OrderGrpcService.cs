using Grpc.Core;
using OrderProto;
using OrderServiceApp.Domain;
using OrderServiceApp.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using AutoMapper;
namespace OrderServiceApp.Services
{
    public class OrderGrpcService : OrderService.OrderServiceBase
    {
        private readonly IOrderRepository _repo;
        private readonly shared.Messaging.RabbitMqPublisher _publisher;
        private readonly ILogger<OrderGrpcService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;

        public OrderGrpcService(IOrderRepository repo, shared.Messaging.RabbitMqPublisher publisher, ILogger<OrderGrpcService> logger, IDistributedCache cache, IMapper mapper)
        {
            _repo = repo;
            _publisher = publisher;
            _logger = logger;
            _cache = cache;
            _mapper = mapper;
        }

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            // Idempotency Check
            string idempotencyKey = $"idempotency_order_{request.UserId}_{request.Amount}_{request.Currency}";
            var existing = await _cache.GetStringAsync(idempotencyKey);
            if (!string.IsNullOrEmpty(existing))
            {
                _logger.LogWarning("Duplicate request detected for key: {IdempotencyKey}", idempotencyKey);
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Duplicate request detected. Please wait a moment."));
            }

            // Set Idempotency Key (expires in 10 seconds)
            await _cache.SetStringAsync(idempotencyKey, "processing", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

            _logger.LogInformation("Creating Order for User {UserId} with Amount {Amount}", request.UserId, request.Amount);

            var order = _mapper.Map<Order>(request);

            var created = await _repo.CreateAsync(order);
            _logger.LogInformation("Order Created: {OrderId}", created.Id);

            // Publish OrderCreatedEvent
            var orderEvent = _mapper.Map<shared.Events.OrderCreatedEvent>(created);

            _logger.LogInformation("Publishing OrderCreatedEvent for Order {OrderId}", created.Id);
            await _publisher.PublishAsync("order_exchange", "order.created", orderEvent);

            return _mapper.Map<CreateOrderResponse>(created);
        }

        public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            string cacheKey = $"order_{request.OrderId}";
            var cachedOrder = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedOrder))
            {
                _logger.LogInformation("Returning Order {OrderId} from Cache", request.OrderId);
                var orderData = JsonConvert.DeserializeObject<Order>(cachedOrder);
                return _mapper.Map<GetOrderResponse>(orderData);
            }

            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

            // Cache the order (expires in 10 minutes)
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(order), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return _mapper.Map<GetOrderResponse>(order);
        }

        public override async Task<UpdateOrderStatusResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
        {
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return new UpdateOrderStatusResponse { Ok = false };

            if (System.Enum.TryParse<OrderStatus>(request.Status, out var s))
                order.Status = s;

            await _repo.UpdateAsync(order);

            // Invalidate Cache
            string cacheKey = $"order_{request.OrderId}";
            await _cache.RemoveAsync(cacheKey);

            return new UpdateOrderStatusResponse { Ok = true };
        }
    }
}
