using Shelfwise.Catalog.Domain.Aggregates;

namespace Shelfwise.Catalog.Application.Abstractions;

public interface IBatchRepository
{
    Task<ProductBatch?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task SaveAsync(ProductBatch batch, CancellationToken cancellationToken = default);
}