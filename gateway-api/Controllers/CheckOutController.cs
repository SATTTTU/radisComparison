using Microsoft.AspNetCore.Mvc;
using order_service.Protos;
using payment_service.Protos;
using shared.Models;

namespace gateway_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly OrderGrpc.OrderGrpcClient _orderClient;
    private readonly PaymentGrpc.PaymentGrpcClient _paymentClient;

    public CheckoutController(OrderGrpc.OrderGrpcClient orderClient, PaymentGrpc.PaymentGrpcClient paymentClient)
    {
        _orderClient = orderClient;
        _paymentClient = paymentClient;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // 1. Create Order in Order Service
        var orderResponse = await _orderClient.CreateOrderAsync(new order_service.Protos.CreateOrderRequest
        {
            Amount = request.Amount
        });

        // 2. Create Payment in Payment Service (PayPal)
        var paymentResponse = await _paymentClient.CreatePaymentAsync(new payment_service.Protos.CreatePaymentRequest
        {
            Amount = request.Amount,
            ReturnUrl = "http://localhost:3000/checkout/success", // Example URL
            CancelUrl = "http://localhost:3000/checkout/cancel"
        });

        return Ok(new
        {
            OrderId = orderResponse.Id,
            PaymentId = paymentResponse.PaymentId,
            ApprovalUrl = paymentResponse.ApprovalUrl
        });
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequest request)
    {
        var response = await _paymentClient.CapturePaymentAsync(new payment_service.Protos.CapturePaymentRequest
        {
            PaymentId = request.PaymentId
        });

        return Ok(new
        {
            PaymentId = response.PaymentId,
            Status = response.Status
        });
    }
}

public class CreateOrderRequest
{
    public double Amount { get; set; }
}

public class CapturePaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
}
