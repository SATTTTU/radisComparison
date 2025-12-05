using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using shared.Events;
using Microsoft.Extensions.Logging;

namespace payment_service.Services;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public OrderCreatedConsumer(IConfiguration configuration, ILogger<OrderCreatedConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
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

        await _channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic);
        await _channel.QueueDeclareAsync("order_created_queue", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync("order_created_queue", "order_exchange", "order.created");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var orderEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(json);

            if (orderEvent != null)
            {
                _logger.LogInformation("OrderCreated event received. OrderId: {OrderId}, Amount: {Amount}",
                    orderEvent.OrderId, orderEvent.Amount);

                // NOTE: Payment will be captured later during PayPal callback
            }

            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync("order_created_queue", autoAck: true, consumer);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}
