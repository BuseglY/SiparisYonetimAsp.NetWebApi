using Microsoft.AspNetCore.Mvc;
using SiparisYonetimSistemi.Models;
using SiparisYonetimSistemi.Services;

namespace SiparisYonetimSistemi.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        [Produces("application/json")]
        public class ProductsController : ControllerBase
        {
            private readonly IProductService _productService;
            private readonly ILogger<ProductsController> _logger;

            public ProductsController(IProductService productService, ILogger<ProductsController> logger)
            {
                _productService = productService;
                _logger = logger;
            }

            /// <summary>
            /// Tüm ürünleri listeler
            /// </summary>
            /// <returns>Ürün listesi</returns>
            [HttpGet]
            [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
            {
                try
                {
                    var products = await _productService.GetProductsAsync();
                    return Ok(products);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving products");
                    return StatusCode(500, "Ürünler getirilirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Belirtilen ID'ye sahip ürünü getirir
            /// </summary>
            /// <param name="id">Ürün ID</param>
            /// <returns>Ürün detayları</returns>
            [HttpGet("{id}")]
            [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<Product>> GetProductById(int id)
            {
                try
                {
                    var product = await _productService.GetProductByIdAsync(id);

                    if (product == null)
                    {
                        return NotFound($"ID'si {id} olan ürün bulunamadı.");
                    }

                    return Ok(product);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", id);
                    return StatusCode(500, "Ürün getirilirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Yeni ürün oluşturur
            /// </summary>
            /// <param name="product">Ürün bilgileri</param>
            /// <returns>Oluşturulan ürün</returns>
            [HttpPost]
            [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    var createdProduct = await _productService.CreateProductAsync(product);

                    return CreatedAtAction(
                        nameof(GetProductById),
                        new { id = createdProduct.Id },
                        createdProduct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    return StatusCode(500, "Ürün oluşturulurken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Ürün stok miktarını günceller
            /// </summary>
            /// <param name="id">Ürün ID</param>
            /// <param name="stock">Yeni stok miktarı</param>
            /// <returns>Güncelleme sonucu</returns>
            [HttpPatch("{id}/stock")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
            {
                try
                {
                    if (stock < 0)
                    {
                        return BadRequest("Stok miktarı negatif olamaz.");
                    }

                    var result = await _productService.UpdateStockAsync(id, stock);

                    if (!result)
                    {
                        return NotFound($"ID'si {id} olan ürün bulunamadı.");
                    }

                    return Ok(new { message = "Stok başarıyla güncellendi.", productId = id, newStock = stock });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating stock for product ID: {ProductId}", id);
                    return StatusCode(500, "Stok güncellenirken bir hata oluştu.");
                }
            }

            /// <summary>
            /// Ürünü siler
            /// </summary>
            /// <param name="id">Ürün ID</param>
            /// <returns>Silme işlemi sonucu</returns>
            [HttpDelete("{id}")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> DeleteProduct(int id)
            {
                try
                {
                    var result = await _productService.DeleteProductAsync(id);

                    if (!result)
                    {
                        return NotFound($"ID'si {id} olan ürün bulunamadı.");
                    }

                    return NoContent();
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                    return StatusCode(500, "Ürün silinirken bir hata oluştu.");
                }
            }
        }
    }
