using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shelfwise.Catalog.Infrastructure.Persistence;
using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Infrastructure.Persistence.Repositories;
using Shelfwise.Catalog.Infrastructure.Time;
using Shelfwise.Catalog.Infrastructure.Persistence.Seeders;

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

        // Scoped: the same lifetime as CatalogDbContext (one per HTTP request)
        services.AddScoped<IBatchRepository, SqlBatchRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICatalogDataSeeder, CatalogDataSeeder>();

        return services;
    }
}