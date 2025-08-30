using SiparisYonetimSistemi.Models;

namespace SiparisYonetimSistemi.Services
{
    

        public interface IProductService
        {
            Task<IEnumerable<Product>> GetProductsAsync();
            Task<Product?> GetProductByIdAsync(int productId);
            Task<Product> CreateProductAsync(Product product);
            Task<Product> UpdateProductAsync(Product product);
            Task<bool> DeleteProductAsync(int productId);
            Task<bool> UpdateStockAsync(int productId, int newStock);
        }

}
