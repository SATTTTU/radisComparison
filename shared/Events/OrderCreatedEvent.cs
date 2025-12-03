namespace shared.Events;

public class OrderCreatedEvent
{
    public int  OrderId { get; set; } 
    public decimal Amount { get; set; }
}
