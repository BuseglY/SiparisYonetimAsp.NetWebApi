using Microsoft.EntityFrameworkCore;
using SiparisYonetimSistemi.Data;
using SiparisYonetimSistemi.DTOs;

namespace SiparisYonetimSistemi.Services
{
    
        public class StockService : IStockService
        {
            private readonly OrderDbContext _context;
            private readonly ILogger<StockService> _logger;
            private static readonly SemaphoreSlim _stockSemaphore = new(1, 1);

            public StockService(OrderDbContext context, ILogger<StockService> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<StockValidationResult> ValidateStockAsync(List<CreateOrderItemDto> items)
            {
                var result = new StockValidationResult { IsValid = true };
                decimal totalValue = 0;

                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new StockValidationError
                        {
                            ProductId = item.ProductId,
                            ProductName = "Unknown Product",
                            AvailableStock = 0,
                            RequestedQuantity = item.Quantity,
                            ErrorType = "ProductNotFound"
                        });
                        continue;
                    }

                    if (product.Stock < item.Quantity)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new StockValidationError
                        {
                            ProductId = item.ProductId,
                            ProductName = product.Name,
                            AvailableStock = product.Stock,
                            RequestedQuantity = item.Quantity,
                            ErrorType = "InsufficientStock"
                        });
                    }
                    else
                    {
                        totalValue += product.Price * item.Quantity;
                    }
                }

                result.TotalValue = totalValue;

                if (!result.IsValid)
                {
                    result.ErrorMessage = string.Join("; ", result.Errors.Select(e =>
                        e.ErrorType == "ProductNotFound"
                            ? $"Product ID {e.ProductId} not found"
                            : $"Insufficient stock for {e.ProductName}: Available {e.AvailableStock}, Requested {e.RequestedQuantity}"));
                }

                return result;
            }

            public async Task<StockValidationResult> ValidateStockWithLockAsync(List<CreateOrderItemDto> items)
            {
                await _stockSemaphore.WaitAsync();

                try
                {
                    var productIds = items.Select(i => i.ProductId).ToList();
                    var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .OrderBy(p => p.Id) // Prevent deadlocks by ordering
                        .ToListAsync();

                    var result = new StockValidationResult { IsValid = true };
                    decimal totalValue = 0;

                    foreach (var item in items)
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                        if (product == null)
                        {
                            result.IsValid = false;
                            result.Errors.Add(new StockValidationError
                            {
                                ProductId = item.ProductId,
                                ProductName = "Unknown Product",
                                AvailableStock = 0,
                                RequestedQuantity = item.Quantity,
                                ErrorType = "ProductNotFound"
                            });
                            continue;
                        }

                        // Check for concurrent stock changes
                        var currentProduct = await _context.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                        if (currentProduct != null && currentProduct.Stock < item.Quantity)
                        {
                            result.IsValid = false;
                            result.Errors.Add(new StockValidationError
                            {
                                ProductId = item.ProductId,
                                ProductName = product.Name,
                                AvailableStock = currentProduct.Stock,
                                RequestedQuantity = item.Quantity,
                                ErrorType = "InsufficientStock"
                            });
                        }
                        else if (currentProduct != null)
                        {
                            totalValue += product.Price * item.Quantity;
                        }
                    }

                    result.TotalValue = totalValue;

                    if (!result.IsValid)
                    {
                        result.ErrorMessage = string.Join("; ", result.Errors.Select(e =>
                            e.ErrorType == "ProductNotFound"
                                ? $"Product ID {e.ProductId} not found"
                                : $"Insufficient stock for {e.ProductName}: Available {e.AvailableStock}, Requested {e.RequestedQuantity}"));
                    }

                    return result;
                }
                finally
                {
                    _stockSemaphore.Release();
                }
            }

            public async Task<bool> ReserveStockAsync(List<CreateOrderItemDto> items)
            {
                await _stockSemaphore.WaitAsync();

                try
                {
                    var validation = await ValidateStockAsync(items);
                    if (!validation.IsValid)
                    {
                        return false;
                    }

                    foreach (var item in items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock -= item.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Stock reserved for {ItemCount} items", items.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reserving stock");
                    return false;
                }
                finally
                {
                    _stockSemaphore.Release();
                }
            }

            public async Task<bool> ReleaseStockAsync(List<CreateOrderItemDto> items)
            {
                await _stockSemaphore.WaitAsync();

                try
                {
                    foreach (var item in items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Stock released for {ItemCount} items", items.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing stock");
                    return false;
                }
                finally
                {
                    _stockSemaphore.Release();
                }
            }

            public async Task<bool> UpdateStockAsync(int productId, int quantity)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                product.Stock = quantity;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock updated for product {ProductId}: {Quantity}", productId, quantity);
                return true;
            }

            public async Task<List<LowStockAlert>> GetLowStockAlertsAsync(int threshold = 5)
            {
                var lowStockProducts = await _context.Products
                    .Where(p => p.Stock <= threshold)
                    .Select(p => new LowStockAlert
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        CurrentStock = p.Stock,
                        Threshold = threshold,
                        AlertDate = DateTime.UtcNow
                    })
                    .ToListAsync();

                return lowStockProducts;
            }

            public async Task<bool> IsStockAvailableAsync(int productId, int requestedQuantity)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productId);

                return product != null && product.Stock >= requestedQuantity;
            }
        }

}
