using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Application.DTOs;
using Shelfwise.Catalog.Domain.Aggregates;
using Shelfwise.Catalog.Domain.Exceptions;

namespace Shelfwise.Catalog.Application.Services;

public sealed class BatchService : IBatchService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IClock _clock;

    public BatchService(IBatchRepository batchRepository, IClock clock)
    {
        _batchRepository = batchRepository;
        _clock = clock;
    }

    public async Task<BatchDto> RegisterBatchAsync(
        Guid productId,
        int quantity,
        DateOnly expiryDate,
        CancellationToken cancellationToken = default)
    {
        var batch = ProductBatch.Register(productId, quantity, expiryDate, _clock.Today);

        await _batchRepository.SaveAsync(batch, cancellationToken);
        return MapToDto(batch);
    }

    public async Task<BatchDto> ReserveStockAsync(
        Guid batchId,
        Guid orderId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var batch = await GetExistingBatchOrThrowAsync(batchId, cancellationToken);

        batch.ReserveStock(orderId, quantity, _clock.Today);

        await _batchRepository.SaveAsync(batch, cancellationToken);
        return MapToDto(batch);
    }

    public async Task<BatchDto?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        return batch is null ? null : MapToDto(batch);
    }

    private async Task<ProductBatch> GetExistingBatchOrThrowAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        return batch ?? throw new BatchNotFoundException(batchId);
    }

    private static BatchDto MapToDto(ProductBatch batch) =>
        new(
            batch.Id,
            batch.ProductId,
            batch.ExpiryDate,
            batch.DeliveredQuantity,
            batch.ReservedQuantity,
            batch.AvailableQuantity,
            batch.Reservations
                .Select(r => new StockReservationDto(r.OrderId, r.Quantity))
                .ToList());
}