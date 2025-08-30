using Microsoft.AspNetCore.Mvc;
using SiparisYonetimSistemi.Services;

namespace SiparisYonetimSistemi.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        [Produces("application/json")]
        public class StockController : ControllerBase
        {
            private readonly IStockService _stockService;
            private readonly ILogger<StockController> _logger;

            public StockController(IStockService stockService, ILogger<StockController> logger)
            {
                _stockService = stockService;
                _logger = logger;
            }

            /// <summary>
            /// Düşük stok uyarılarını getirir
            /// </summary>
            /// <param name="threshold">Stok eşik değeri (varsayılan: 5)</param>
            /// <returns>Düşük stok uyarı listesi</returns>
            [HttpGet("low-stock-alerts")]
            [ProducesResponseType(typeof(IEnumerable<LowStockAlert>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<IEnumerable<LowStockAlert>>> GetLowStockAlerts([FromQuery] int threshold = 5)
            {
                try
                {
                    var alerts = await _stockService.GetLowStockAlertsAsync(threshold);
                    return Ok(alerts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving low stock alerts");
                    return StatusCode(500, "Düşük stok uyarıları getirilirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Belirtilen ürün için stok durumunu kontrol eder
            /// </summary>
            /// <param name="productId">Ürün ID</param>
            /// <param name="quantity">İstenen miktar</param>
            /// <returns>Stok durumu</returns>
            [HttpGet("check-availability/{productId}")]
            [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult> CheckStockAvailability(int productId, [FromQuery] int quantity = 1)
            {
                try
                {
                    var isAvailable = await _stockService.IsStockAvailableAsync(productId, quantity);
                    return Ok(new { productId, requestedQuantity = quantity, isAvailable });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking stock availability for product {ProductId}", productId);
                    return StatusCode(500, "Stok durumu kontrol edilirken bir hata oluştu.");
                }
            }
        }
    }

