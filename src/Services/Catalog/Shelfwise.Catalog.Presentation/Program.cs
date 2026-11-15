using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Infrastructure; 
using Shelfwise.Catalog.Presentation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // turn on controllers
builder.Services.AddOpenApi(); // Api 

// Composition Root: wiring ports to implementations
builder.Services.AddSingleton<IBatchRepository, InMemoryBatchRepository>(); 
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IBatchService, BatchService>();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();