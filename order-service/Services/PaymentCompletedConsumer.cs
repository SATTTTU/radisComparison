using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using shared.Events;
using OrderServiceApp.Repositories;
using OrderServiceApp.Domain;

namespace OrderServiceApp.Services;

public class PaymentCompletedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentCompletedConsumer> _logger;
    private IConnection _connection;
    private IChannel _channel;

    public PaymentCompletedConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<PaymentCompletedConsumer> logger)
    {
        _serviceProvider = serviceProvider;
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

        await _channel.ExchangeDeclareAsync(exchange: "payment_exchange", type: ExchangeType.Topic);
        await _channel.QueueDeclareAsync(queue: "payment_completed_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        await _channel.QueueBindAsync(queue: "payment_completed_queue", exchange: "payment_exchange", routingKey: "payment.completed");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var paymentEvent = JsonConvert.DeserializeObject<PaymentCompletedEvent>(message);

            if (paymentEvent != null)
            {
                _logger.LogInformation("Received PaymentCompletedEvent for Order {OrderId} with Status {Status}", paymentEvent.OrderId, paymentEvent.Status);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var order = await repo.GetByIdAsync(paymentEvent.OrderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Completed;
                        await repo.UpdateAsync(order);
                        _logger.LogInformation("Order {OrderId} status updated to Completed", paymentEvent.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning("Order {OrderId} not found", paymentEvent.OrderId);
                    }
                }
            }
        };

        await _channel.BasicConsumeAsync(queue: "payment_completed_queue", autoAck: true, consumer: consumer);
        
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
