using Shelfwise.Catalog.Domain.Entities;
using Shelfwise.Catalog.Domain.Exceptions;

namespace Shelfwise.Catalog.Domain.Aggregates;

public sealed class ProductBatch
{
    private readonly List<StockReservation> _reservations = new();

    public Guid Id { get; }
    public Guid ProductId { get; }
    public DateOnly ExpiryDate { get; }
    public int DeliveredQuantity { get; }

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();
    public int ReservedQuantity => _reservations.Sum(r => r.Quantity);
    public int AvailableQuantity => DeliveredQuantity - ReservedQuantity;

    private ProductBatch(Guid id, Guid productId, int deliveredQuantity, DateOnly expiryDate)
    {
        Id = id;
        ProductId = productId;
        DeliveredQuantity = deliveredQuantity;
        ExpiryDate = expiryDate;
    }

    // Command: Register batch -> Event: Batch delivered (+ Batch expiry date recorded)
    public static ProductBatch Register(Guid productId, int quantity, DateOnly expiryDate, DateOnly today)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

        if (quantity <= 0)
            throw new InvalidQuantityException(quantity);

        if (expiryDate <= today)
            throw new InvalidExpiryDateException(expiryDate, today);

        return new ProductBatch(Guid.NewGuid(), productId, quantity, expiryDate);
    }

    public bool IsExpiredOn(DateOnly date) => date > ExpiryDate;

    // Command: Reserve stock -> Event: Stock reserved
    public void ReserveStock(Guid orderId, int quantity, DateOnly today)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        if (quantity <= 0)
            throw new InvalidQuantityException(quantity);

        if (IsExpiredOn(today))
            throw new BatchExpiredException(Id, ExpiryDate);

        if (quantity > AvailableQuantity)
            throw new InsufficientStockException(quantity, AvailableQuantity);

        var existing = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            return;
        }

        _reservations.Add(new StockReservation(orderId, quantity));
    }

    // Command: Release reservation -> Event: Stock reservation released
    // releasing a reservation that does not exist changes nothing.
    public void ReleaseReservation(Guid orderId)
    {
        _reservations.RemoveAll(r => r.OrderId == orderId);
    }
}