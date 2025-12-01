namespace order_service.Domain;

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

pubic enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}