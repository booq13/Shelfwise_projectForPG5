using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Domain.Aggregates;

namespace Shelfwise.Catalog.Infrastructure.Persistence.Seeders;

internal sealed class CatalogDataSeeder : ICatalogDataSeeder
{
    // Fixed ids make manual testing easier
    private static readonly Guid MilkProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid YogurtProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoOrderId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly CatalogDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ILogger<CatalogDataSeeder> _logger;

    public CatalogDataSeeder(CatalogDbContext dbContext, IClock clock, ILogger<CatalogDataSeeder> logger)
    {
        _dbContext = dbContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Prepare the database for the active provider
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        // Idempotency: if there are already batches, do nothing
        if (await _dbContext.Batches.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Database already contains batches. Skipping CatalogDataSeeder.");
            return;
        }

        var today = _clock.Today;

        
        var milkBatch = ProductBatch.Register(MilkProductId, quantity: 50, today.AddDays(30), today);

        var yogurtBatch = ProductBatch.Register(YogurtProductId, quantity: 5, today.AddDays(3), today);
        yogurtBatch.ReserveStock(DemoOrderId, quantity: 5, today);

        await _dbContext.Batches.AddRangeAsync([milkBatch, yogurtBatch], cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded 2 batches. MilkBatchId: {MilkBatchId}, SoldOutYogurtBatchId: {YogurtBatchId}",
            milkBatch.Id,
            yogurtBatch.Id);
    }
}