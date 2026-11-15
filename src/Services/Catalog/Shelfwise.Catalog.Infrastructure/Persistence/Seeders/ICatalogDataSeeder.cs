namespace Shelfwise.Catalog.Infrastructure.Persistence.Seeders;

public interface ICatalogDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}