using payment_service.Services;
using payment_service.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));
builder.Services.AddSingleton<IPayPalService, PayPalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PaymentGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
