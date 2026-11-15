using Microsoft.EntityFrameworkCore;
using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Domain.Aggregates;

namespace Shelfwise.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class SqlBatchRepository : IBatchRepository
{
    private readonly CatalogDbContext _dbContext;

    public SqlBatchRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductBatch?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Batches
            .Include(b => b.Reservations)
            .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductBatch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Batches
            .AsNoTracking()
            .Include(b => b.Reservations)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        
        if (_dbContext.Entry(batch).State == EntityState.Detached)
        {
            await _dbContext.Batches.AddAsync(batch, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        _dbContext.Batches.Remove(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}