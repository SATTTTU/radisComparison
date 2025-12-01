using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using payment_service.Config;

namespace payment_service.Services;

public interface IPayPalService
{
    Task<(string Id, string ApprovalUrl)> CreateOrderAsync(double amount, string returnUrl, string cancelUrl);
    Task<string> CaptureOrderAsync(string orderId);
}

public class PayPalService : IPayPalService
{
    private readonly PayPalHttpClient _client;

    public PayPalService(IOptions<PayPalOptions> options)
    {
        PayPalEnvironment environment;
        if (options.Value.Mode == "Live")
        {
            environment = new LiveEnvironment(options.Value.ClientId, options.Value.ClientSecret);
        }
        else
        {
            environment = new SandboxEnvironment(options.Value.ClientId, options.Value.ClientSecret);
        }

        _client = new PayPalHttpClient(environment);
    }

    public async Task<(string Id, string ApprovalUrl)> CreateOrderAsync(double amount, string returnUrl, string cancelUrl)
    {
        var orderRequest = new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext
            {
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            },
            PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        Value = amount.ToString("F2")
                    }
                }
            }
        };

        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(orderRequest);

        var response = await _client.Execute(request);
        var order = response.Result<Order>();

        var approvalUrl = order.Links.FirstOrDefault(x => x.Rel == "approve")?.Href;

        return (order.Id, approvalUrl ?? string.Empty);
    }

    public async Task<string> CaptureOrderAsync(string orderId)
    {
        var request = new OrdersCaptureRequest(orderId);
        request.RequestBody(new OrderActionRequest());

        var response = await _client.Execute(request);
        var order = response.Result<Order>();

        return order.Status;
    }
}
