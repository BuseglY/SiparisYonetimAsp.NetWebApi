using Microsoft.AspNetCore.Mvc;
using SiparisYonetimSistemi.DTOs;
using SiparisYonetimSistemi.Models;
using SiparisYonetimSistemi.Services;

namespace SiparisYonetimSistemi.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        [Produces("application/json")]
        public class OrdersController : ControllerBase
        {
            private readonly IOrderService _orderService;
            private readonly ILogger<OrdersController> _logger;

            public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
            {
                _orderService = orderService;
                _logger = logger;
            }

            /// <summary>
            /// Yeni sipariş oluşturur
            /// </summary>
            /// <param name="createOrderDto">Sipariş bilgileri</param>
            /// <returns>Oluşturulan sipariş</returns>
            [HttpPost]
            [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
            [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    var order = await _orderService.CreateOrderAsync(createOrderDto);

                    _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);

                    return CreatedAtAction(
                        nameof(GetOrderById),
                        new { id = order.Id },
                        order);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("Invalid operation while creating order: {Message}", ex.Message);
                    return BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Invalid argument while creating order: {Message}", ex.Message);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while creating order");
                    return StatusCode(500, "Sipariş oluşturulurken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Tüm siparişleri listeler
            /// </summary>
            /// <returns>Sipariş listesi</returns>
            [HttpGet]
            [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
            {
                try
                {
                    var orders = await _orderService.GetOrdersAsync();
                    return Ok(orders);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving orders");
                    return StatusCode(500, "Siparişler getirilirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Belirtilen ID'ye sahip siparişin detaylarını getirir
            /// </summary>
            /// <param name="id">Sipariş ID</param>
            /// <returns>Sipariş detayları</returns>
            [HttpGet("{id}")]
            [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<OrderDto>> GetOrderById(int id)
            {
                try
                {
                    var order = await _orderService.GetOrderByIdAsync(id);

                    if (order == null)
                    {
                        _logger.LogWarning("Order with ID {OrderId} not found", id);
                        return NotFound($"ID'si {id} olan sipariş bulunamadı.");
                    }

                    return Ok(order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving order with ID: {OrderId}", id);
                    return StatusCode(500, "Sipariş getirilirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Belirtilen ID'ye sahip siparişi siler
            /// </summary>
            /// <param name="id">Sipariş ID</param>
            /// <returns>Silme işlemi sonucu</returns>
            [HttpDelete("{id}")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> DeleteOrder(int id)
            {
                try
                {
                    var result = await _orderService.DeleteOrderAsync(id);

                    if (!result)
                    {
                        _logger.LogWarning("Attempted to delete non-existent order with ID: {OrderId}", id);
                        return NotFound($"ID'si {id} olan sipariş bulunamadı.");
                    }

                    _logger.LogInformation("Order with ID {OrderId} deleted successfully", id);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting order with ID: {OrderId}", id);
                    return StatusCode(500, "Sipariş silinirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Sipariş durumunu günceller
            /// </summary>
            /// <param name="id">Sipariş ID</param>
            /// <param name="status">Yeni durum</param>
            /// <returns>Güncellenmiş sipariş</returns>
            [HttpPatch("{id}/status")]
            [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
            {
                try
                {
                    var order = await _orderService.UpdateOrderStatusAsync(id, status);

                    _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);
                    return Ok(order);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Invalid argument while updating order status: {Message}", ex.Message);
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order status for ID: {OrderId}", id);
                    return StatusCode(500, "Sipariş durumu güncellenirken bir hata oluştu.");
                }
            }
        }
    }
