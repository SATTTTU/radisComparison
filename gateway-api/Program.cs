using OrderProto;
using PaymentProto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS ---
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

// GRPC Clients
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5236");
});

builder.Services.AddGrpcClient<PaymentService.PaymentServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5116");
});


var app = builder.Build();

// --- MIDDLEWARE ORDER MATTERS ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS BEFORE authorization
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Map controllers AFTER middleware
app.MapControllers();

app.Run();
