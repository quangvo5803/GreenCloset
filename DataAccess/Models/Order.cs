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

        public double TotalPrice { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } // "PayByCash" hoặc "VNPay"

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DeliveryOption DeliveryOption { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
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
