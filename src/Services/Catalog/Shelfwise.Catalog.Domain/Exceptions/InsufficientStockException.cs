namespace Shelfwise.Catalog.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(int requestedQuantity, int availableQuantity)
        : base(
            code: "batch.insufficient_stock",
            message: $"Cannot reserve {requestedQuantity} items. Only {availableQuantity} available in this batch.")
    {
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}