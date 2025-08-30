using System.ComponentModel.DataAnnotations;

namespace SiparisYonetimSistemi.Models
{   
        public class Order
        {
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string CustomerName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string CustomerEmail { get; set; } = string.Empty;

            [StringLength(500)]
            public string ShippingAddress { get; set; } = string.Empty;

            [Required]
            public decimal TotalAmount { get; set; }

            public OrderStatus Status { get; set; } = OrderStatus.Pending;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }

            // Navigation property
            public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        }

        public enum OrderStatus
        {
            Pending = 0,
            Confirmed = 1,
            Processing = 2,
            Shipped = 3,
            Delivered = 4,
            Cancelled = 5
        }
    }
