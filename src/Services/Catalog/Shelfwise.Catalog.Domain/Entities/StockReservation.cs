using Shelfwise.Catalog.Domain.Exceptions;

namespace Shelfwise.Catalog.Domain.Entities;

public sealed class StockReservation
{
    public Guid OrderId { get; }
    public int Quantity { get; private set; }

    internal StockReservation(Guid orderId, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        if (quantity <= 0)
            throw new InvalidQuantityException(quantity);

        OrderId = orderId;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int additionalQuantity)
    {
        if (additionalQuantity <= 0)
            throw new InvalidQuantityException(additionalQuantity);

        Quantity += additionalQuantity;
    }
}