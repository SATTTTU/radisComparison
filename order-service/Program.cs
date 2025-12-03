using Microsoft.EntityFrameworkCore;
using OrderServiceApp.Data;
using OrderServiceApp.Repositories;
using OrderServiceApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DB -- use connection string from appsettings or environment
var connection = builder.Configuration.GetConnectionString("OrderDatabase")
    ?? builder.Configuration["ORDER_DB_CONNECTION"]
    ?? "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=myname";


builder.Services.AddDbContext<OrderDbContext>(opts => opts.UseNpgsql(connection));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<shared.Messaging.RabbitMqPublisher>();
builder.Services.AddHostedService<PaymentCompletedConsumer>();
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<OrderGrpcService>();
app.MapGet("/", () => "Order service running");
app.UseCors("AllowFrontend");

app.Run();
