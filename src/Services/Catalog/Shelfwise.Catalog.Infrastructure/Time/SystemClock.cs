using Shelfwise.Catalog.Application.Abstractions;

namespace Shelfwise.Catalog.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}