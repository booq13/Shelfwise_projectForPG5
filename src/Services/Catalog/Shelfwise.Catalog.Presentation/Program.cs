using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Presentation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Composition Root: wiring ports to implementations
builder.Services.AddSingleton<IBatchRepository, InMemoryBatchRepository>(); // Singleton: data survives between requests
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IBatchService, BatchService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();