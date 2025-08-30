using System.ComponentModel.DataAnnotations;

namespace SiparisYonetimSistemi.DTOs
{
        public class CreateOrderDto
        {
            [Required]
            [StringLength(100)]
            public string CustomerName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string CustomerEmail { get; set; } = string.Empty;

            [StringLength(500)]
            public string ShippingAddress { get; set; } = string.Empty;

            [Required]
            [MinLength(1, ErrorMessage = "At least one item is required")]
            public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
        }

        public class CreateOrderItemDto
        {
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
            public int ProductId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }
        }
    }
