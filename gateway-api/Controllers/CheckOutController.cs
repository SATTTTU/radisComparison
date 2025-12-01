using Microsoft.AspNetCore.Mvc;
using OrderProto;   // Defined in order.proto
using PaymentProto; // Defined in payment.proto

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    // FIX 1: The type is ServiceName + "Client"
    // It is NOT "OrderGrpcClient"
    private readonly OrderService.OrderServiceClient _orderClient;
    private readonly PaymentService.PaymentServiceClient _paymentClient;

    public CheckoutController(
        // FIX 2: Update Constructor Injection types
        OrderService.OrderServiceClient orderClient,
        PaymentService.PaymentServiceClient paymentClient)
    {
        _orderClient = orderClient;
        _paymentClient = paymentClient;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        // 1. Order Service gRPC Call
        var orderResponse = await _orderClient.CreateOrderAsync(new CreateOrderRequest
        {
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency
        });

        // 2. Payment Service gRPC Call
        var paymentResponse = await _paymentClient.CreatePaymentAsync(new CreatePaymentRequest
        {
            Amount = request.Amount,
            // FIX 3: REMOVE 'Currency'. Your payment.proto 'CreatePaymentRequest' 
            // does not have a Currency field, only amount, returnUrl, and cancelUrl.
            // Currency = request.Currency, 

            ReturnUrl = "http://localhost:3000/checkout/success",
            CancelUrl = "http://localhost:3000/checkout/cancel"
        });

        return Ok(new
        {
            OrderId = orderResponse.OrderId,
            PaymentId = paymentResponse.PaymentId,
            ApprovalUrl = paymentResponse.ApprovalUrl
        });
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentDto request)
    {
        var response = await _paymentClient.CapturePaymentAsync(new CapturePaymentRequest
        {
            PaymentId = request.PaymentId
        });

        return Ok(response);
    }
}

public class CreateOrderDto
{
    public int UserId { get; set; }
    public long Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class CapturePaymentDto
{
    public string PaymentId { get; set; } = string.Empty;
}