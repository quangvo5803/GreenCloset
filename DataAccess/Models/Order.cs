using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? CompleteDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CancelDate { get; set; }

        public double TotalPrice { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } // "PayByCash" hoặc "VNPay"

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DeliveryOption DeliveryOption { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CancelReason {  get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual ICollection<Feedback>? Feedbacks { get; set; }

    }

    public enum OrderStatus
    {
        Pending,
        Delivering,
        Completed,
        Cancelled,
    }

    public enum PaymentMethod
    {
        PayByCash,
        VNPay,
        MoMo
    }

    public enum DeliveryOption
    {
        HomeDelivery,
        StorePickup
    }
}
