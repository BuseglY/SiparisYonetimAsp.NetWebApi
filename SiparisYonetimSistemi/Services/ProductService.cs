using Microsoft.EntityFrameworkCore;
using SiparisYonetimSistemi.Data;
using SiparisYonetimSistemi.Models;

namespace SiparisYonetimSistemi.Services
{
        public class ProductService : IProductService
        {
            private readonly OrderDbContext _context;
            private readonly ILogger<ProductService> _logger;

            public ProductService(OrderDbContext context, ILogger<ProductService> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<IEnumerable<Product>> GetProductsAsync()
            {
                return await _context.Products
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }

            public async Task<Product?> GetProductByIdAsync(int productId)
            {
                return await _context.Products.FindAsync(productId);
            }

            public async Task<Product> CreateProductAsync(Product product)
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductName} created with ID {ProductId}", product.Name, product.Id);
                return product;
            }

            public async Task<Product> UpdateProductAsync(Product product)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated successfully", product.Id);
                return product;
            }

            public async Task<bool> DeleteProductAsync(int productId)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Check if product is used in any orders
                var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId);
                if (hasOrders)
                {
                    throw new InvalidOperationException("Cannot delete product that has been ordered");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} deleted successfully", productId);
                return true;
            }

            public async Task<bool> UpdateStockAsync(int productId, int newStock)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                product.Stock = newStock;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} stock updated to {Stock}", productId, newStock);
                return true;
            }
        }

}
