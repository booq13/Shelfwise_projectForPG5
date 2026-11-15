using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Infrastructure;
using Shelfwise.Catalog.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application layer: unchanged since Lab 3
builder.Services.AddScoped<IBatchService, BatchService>();

// Infrastructure layer: CatalogDbContext, SqlBatchRepository, SystemClock, CatalogDataSeeder
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ICatalogDataSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();