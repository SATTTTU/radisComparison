using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using shared.Events;
using shared.Messaging;
using AutoMapper;

namespace payment_service.Services;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IMapper _mapper;
    private IConnection _connection;
    private IChannel _channel;

    public OrderCreatedConsumer(IConfiguration configuration, RabbitMqPublisher publisher, ILogger<OrderCreatedConsumer> logger, IMapper mapper)
    {
        _configuration = configuration;
        _publisher = publisher;
        _logger = logger;
        _mapper = mapper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange: "order_exchange", type: ExchangeType.Topic);
        await _channel.QueueDeclareAsync(queue: "order_created_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        await _channel.QueueBindAsync(queue: "order_created_queue", exchange: "order_exchange", routingKey: "order.created");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var orderEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(message);

            if (orderEvent != null)
            {
                _logger.LogInformation("Received OrderCreatedEvent for Order {OrderId}", orderEvent.OrderId);

                // Simulate payment processing
                _logger.LogInformation("Processing payment for Order {OrderId}...", orderEvent.OrderId);
                await Task.Delay(1000); // Simulate work

                // Publish PaymentCompletedEvent
                var paymentEvent = _mapper.Map<PaymentCompletedEvent>(orderEvent);

                _logger.LogInformation("Payment processed. Publishing PaymentCompletedEvent for Order {OrderId}", orderEvent.OrderId);
                await _publisher.PublishAsync("payment_exchange", "payment.completed", paymentEvent);
            }
        };

        await _channel.BasicConsumeAsync(queue: "order_created_queue", autoAck: true, consumer: consumer);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}
