using Shelfwise.Catalog.Application.Abstractions;

namespace Shelfwise.Catalog.Presentation.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}