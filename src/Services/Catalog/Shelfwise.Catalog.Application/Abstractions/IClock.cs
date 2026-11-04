namespace Shelfwise.Catalog.Application.Abstractions;

public interface IClock
{
    DateOnly Today { get; }
}