using SiparisYonetimSistemi.DTOs;

namespace SiparisYonetimSistemi.Services
{
    
        public interface IStockService
        {
            Task<StockValidationResult> ValidateStockAsync(List<CreateOrderItemDto> items);
            Task<StockValidationResult> ValidateStockWithLockAsync(List<CreateOrderItemDto> items);
            Task<bool> ReserveStockAsync(List<CreateOrderItemDto> items);
            Task<bool> ReleaseStockAsync(List<CreateOrderItemDto> items);
            Task<bool> UpdateStockAsync(int productId, int quantity);
            Task<List<LowStockAlert>> GetLowStockAlertsAsync(int threshold = 5);
            Task<bool> IsStockAvailableAsync(int productId, int requestedQuantity);
        }

        public class StockValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public List<StockValidationError> Errors { get; set; } = new List<StockValidationError>();
            public decimal TotalValue { get; set; }
        }

        public class StockValidationError
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int AvailableStock { get; set; }
            public int RequestedQuantity { get; set; }
            public string ErrorType { get; set; } = string.Empty;
        }

        public class LowStockAlert
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int CurrentStock { get; set; }
            public int Threshold { get; set; }
            public DateTime AlertDate { get; set; }
        }

}
