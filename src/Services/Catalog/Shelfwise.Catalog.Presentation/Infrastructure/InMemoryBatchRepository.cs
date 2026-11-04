using System.Collections.Concurrent;
using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Domain.Aggregates;

namespace Shelfwise.Catalog.Presentation.Infrastructure;

public sealed class InMemoryBatchRepository : IBatchRepository
{
    private readonly ConcurrentDictionary<Guid, ProductBatch> _storage = new();

    public Task<ProductBatch?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(batchId, out var batch);
        return Task.FromResult(batch);
    }

    public Task SaveAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        _storage[batch.Id] = batch;
        return Task.CompletedTask;
    }
}