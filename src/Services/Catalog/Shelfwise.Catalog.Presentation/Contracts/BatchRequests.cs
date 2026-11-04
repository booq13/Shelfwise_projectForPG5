namespace Shelfwise.Catalog.Presentation.Contracts;

public sealed record RegisterBatchRequest(
    Guid ProductId,
    int Quantity,
    DateOnly ExpiryDate);

public sealed record ReserveStockRequest(
    Guid OrderId,
    int Quantity);