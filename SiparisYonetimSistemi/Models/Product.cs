using System.ComponentModel.DataAnnotations;

namespace SiparisYonetimSistemi.Models
{
        public class Product
        {
            public int Id { get; set; }

            [Required]
            [StringLength(200)]
            public string Name { get; set; } = string.Empty;

            [StringLength(1000)]
            public string Description { get; set; } = string.Empty;

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
            public decimal Price { get; set; }

            [Required]
            [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
            public int Stock { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            // Navigation property
            public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        }

}
