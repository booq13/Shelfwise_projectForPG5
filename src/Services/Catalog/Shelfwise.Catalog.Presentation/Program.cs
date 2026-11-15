using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application layer: unchanged since Lab 3
builder.Services.AddScoped<IBatchService, BatchService>();

// Infrastructure layer: CatalogDbContext, SqlBatchRepository, SystemClock
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();