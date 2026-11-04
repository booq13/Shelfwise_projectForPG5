using Shelfwise.Catalog.Application.DTOs;

namespace Shelfwise.Catalog.Application.Services;

public interface IBatchService
{
    // Command: Register batch
    Task<BatchDto> RegisterBatchAsync(Guid productId, int quantity, DateOnly expiryDate, CancellationToken cancellationToken = default);

    // Command: Reserve stock
    Task<BatchDto> ReserveStockAsync(Guid batchId, Guid orderId, int quantity, CancellationToken cancellationToken = default);

    // Query: Available stock per batch
    Task<BatchDto?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
}