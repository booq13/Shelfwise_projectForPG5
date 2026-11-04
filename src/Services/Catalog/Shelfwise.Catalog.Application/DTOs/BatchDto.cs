namespace Shelfwise.Catalog.Application.DTOs;

public sealed record StockReservationDto(
    Guid OrderId,
    int Quantity);

public sealed record BatchDto(
    Guid BatchId,
    Guid ProductId,
    DateOnly ExpiryDate,
    int DeliveredQuantity,
    int ReservedQuantity,
    int AvailableQuantity,
    IReadOnlyList<StockReservationDto> Reservations);