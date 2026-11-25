using Shelfwise.Catalog.Domain.Exceptions;

namespace Shelfwise.Catalog.Domain.Entities;

public sealed class StockReservation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public int Quantity { get; private set; }
    public DateOnly ReservedOn { get; private set; }

    // Used only by EF Core
    private StockReservation()
    {
    }

    internal StockReservation(Guid orderId, int quantity, DateOnly reservedOn)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        if (quantity <= 0)
            throw new InvalidQuantityException(quantity);

        Id = Guid.NewGuid();
        OrderId = orderId;
        Quantity = quantity;
        ReservedOn = reservedOn;
    }

    // The same reservation grows: Id and ReservedOn stay unchanged
    internal void IncreaseQuantity(int additionalQuantity)
    {
        if (additionalQuantity <= 0)
            throw new InvalidQuantityException(additionalQuantity);

        Quantity += additionalQuantity;
    }
}
