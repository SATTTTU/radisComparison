using System;

namespace OrderServiceApp.Domain
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Failed,
        Cancelled,
        Completed
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long Amount { get; set; } // cents
        public string Currency { get; set; } = "USD";
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
