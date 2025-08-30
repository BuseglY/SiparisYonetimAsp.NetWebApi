using System.ComponentModel.DataAnnotations;

namespace SiparisYonetimSistemi.Models
{
    
        public class OrderItem
        {
            public int Id { get; set; }

            [Required]
            public int OrderId { get; set; }

            [Required]
            public int ProductId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            [Required]
            public decimal UnitPrice { get; set; }

            public decimal TotalPrice => Quantity * UnitPrice;

            // Navigation properties
            public virtual Order Order { get; set; } = null!;
            public virtual Product Product { get; set; } = null!;
        }
    }
