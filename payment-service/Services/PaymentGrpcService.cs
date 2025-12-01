using Grpc.Core;
using payment_service.Protos;

namespace payment_service.Services;

public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
{
    private readonly IPayPalService _payPalService;
    private readonly ILogger<PaymentGrpcService> _logger;

    public PaymentGrpcService(IPayPalService payPalService, ILogger<PaymentGrpcService> logger)
    {
        _payPalService = payPalService;
        _logger = logger;
    }

    public override async Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        try
        {
            var (orderId, approvalUrl) = await _payPalService.CreateOrderAsync(request.Amount, request.ReturnUrl, request.CancelUrl);

            return new CreatePaymentResponse
            {
                PaymentId = orderId,
                ApprovalUrl = approvalUrl,
                Status = "Created"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal order");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create payment"));
        }
    }

    public override async Task<CapturePaymentResponse> CapturePayment(CapturePaymentRequest request, ServerCallContext context)
    {
        try
        {
            var status = await _payPalService.CaptureOrderAsync(request.PaymentId);

            return new CapturePaymentResponse
            {
                PaymentId = request.PaymentId,
                Status = status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PayPal order");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to capture payment"));
        }
    }
}
