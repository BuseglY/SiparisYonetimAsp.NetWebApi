using Microsoft.EntityFrameworkCore;
using SiparisYonetimSistemi.Data;
using SiparisYonetimSistemi.DTOs;
using SiparisYonetimSistemi.Models;

namespace SiparisYonetimSistemi.Services
{
        public class OrderService : IOrderService
        {
            private readonly OrderDbContext _context;
            private readonly IStockService _stockService;
            private readonly ILogger<OrderService> _logger;

            public OrderService(OrderDbContext context, IStockService stockService, ILogger<OrderService> logger)
            {
                _context = context;
                _stockService = stockService;
                _logger = logger;
            }

            public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var stockValidation = await _stockService.ValidateStockWithLockAsync(createOrderDto.Items);
                    if (!stockValidation.IsValid)
                    {
                        throw new InvalidOperationException($"Stock validation failed: {stockValidation.ErrorMessage}");
                    }

                    // Create order
                    var order = new Order
                    {
                        CustomerName = createOrderDto.CustomerName,
                        CustomerEmail = createOrderDto.CustomerEmail,
                        ShippingAddress = createOrderDto.ShippingAddress,
                        Status = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        TotalAmount = stockValidation.TotalValue
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var stockReserved = await _stockService.ReserveStockAsync(createOrderDto.Items);
                    if (!stockReserved)
                    {
                        throw new InvalidOperationException("Failed to reserve stock for the order");
                    }

                    // Create order items
                    var orderItems = new List<OrderItem>();
                    foreach (var itemDto in createOrderDto.Items)
                    {
                        var product = await _context.Products.FindAsync(itemDto.ProductId);
                        if (product == null)
                        {
                            throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
                        }

                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = product.Price
                        };

                        orderItems.Add(orderItem);
                    }

                    _context.OrderItems.AddRange(orderItems);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerEmail}",
                        order.Id, order.CustomerEmail);

                    return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to retrieve created order");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating order for customer {CustomerEmail}", createOrderDto.CustomerEmail);
                    throw;
                }
            }

            public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return orders.Select(MapToOrderDto);
            }

            public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                return order != null ? MapToOrderDto(order) : null;
            }

            public async Task<bool> DeleteOrderAsync(int orderId)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order == null)
                    {
                        return false;
                    }

                    if (order.Status != OrderStatus.Delivered)
                    {
                        var itemsToRelease = order.OrderItems.Select(oi => new CreateOrderItemDto
                        {
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity
                        }).ToList();

                        await _stockService.ReleaseStockAsync(itemsToRelease);
                    }

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Order {OrderId} deleted successfully", orderId);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting order {OrderId}", orderId);
                    throw;
                }
            }

            public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status)
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    throw new ArgumentException($"Order with ID {orderId} not found");
                }

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

                return await GetOrderByIdAsync(orderId) ?? throw new InvalidOperationException("Failed to retrieve updated order");
            }

            private static OrderDto MapToOrderDto(Order order)
            {
                return new OrderDto
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    ShippingAddress = order.ShippingAddress,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                };
            }
        }

}
