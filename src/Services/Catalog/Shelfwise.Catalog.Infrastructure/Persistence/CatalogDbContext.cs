using Microsoft.EntityFrameworkCore;
using Shelfwise.Catalog.Domain.Aggregates;

namespace Shelfwise.Catalog.Infrastructure.Persistence;

internal sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductBatch> Batches => Set<ProductBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}