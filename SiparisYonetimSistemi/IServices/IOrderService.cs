using SiparisYonetimSistemi.DTOs;
using SiparisYonetimSistemi.Models;

namespace SiparisYonetimSistemi.Services
{
        public interface IOrderService
        {
            Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
            Task<IEnumerable<OrderDto>> GetOrdersAsync();
            Task<OrderDto?> GetOrderByIdAsync(int orderId);
            Task<bool> DeleteOrderAsync(int orderId);
            Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        }

}
