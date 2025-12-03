namespace shared.Events;

public class PaymentCompletedEvent
{
    public int  OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
