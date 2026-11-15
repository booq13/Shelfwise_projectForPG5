using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shelfwise.Catalog.Infrastructure.Persistence;

namespace Shelfwise.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("Persistence:UseInMemoryDatabase");

        services.AddDbContext<CatalogDbContext>(options =>
        {
            if (useInMemory)
            {
                var dbName = configuration["Persistence:InMemoryDatabaseName"] ?? "ShelfwiseCatalogInMemoryDb";
                options.UseInMemoryDatabase(dbName);
            }
            else
            {
                var connectionString = configuration.GetConnectionString("CatalogDb")
                    ?? throw new InvalidOperationException("Missing connection string 'CatalogDb' in configuration.");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                });
            }
        });

        return services;
    }
}