using Shelfwise.Catalog.Domain.Aggregates;
using Shelfwise.Catalog.Domain.Exceptions;
using Xunit;

namespace Shelfwise.Catalog.Domain.Tests;

public class ProductBatchTests
{
    private static readonly DateOnly Today = new(2026, 11, 4);
    private static readonly DateOnly ExpiryInTenDays = Today.AddDays(10);

    private static ProductBatch CreateBatch(int quantity = 100, DateOnly? expiryDate = null) =>
        ProductBatch.Register(Guid.NewGuid(), quantity, expiryDate ?? ExpiryInTenDays, Today);

    // Register batch -> Batch delivered 

    [Fact]
    public void Register_WhenValidData_CreatesBatchWithFullAvailableStock()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var batch = ProductBatch.Register(productId, quantity: 50, ExpiryInTenDays, Today);

        // Assert
        Assert.NotEqual(Guid.Empty, batch.Id);
        Assert.Equal(productId, batch.ProductId);
        Assert.Equal(ExpiryInTenDays, batch.ExpiryDate);
        Assert.Equal(50, batch.DeliveredQuantity);
        Assert.Equal(0, batch.ReservedQuantity);
        Assert.Equal(50, batch.AvailableQuantity);
        Assert.Empty(batch.Reservations);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Register_WhenQuantityIsNotPositive_ThrowsInvalidQuantityException(int invalidQuantity)
    {
        // Act
        var ex = Assert.Throws<InvalidQuantityException>(() =>
            ProductBatch.Register(Guid.NewGuid(), invalidQuantity, ExpiryInTenDays, Today));

        // Assert
        Assert.Equal("batch.invalid_quantity", ex.Code);
        Assert.Equal(invalidQuantity, ex.AttemptedQuantity);
    }

    [Theory]
    [InlineData(0)]   // expire tofay
    [InlineData(-1)] // already expire
    public void Register_WhenExpiryDateIsNotInTheFuture_ThrowsInvalidExpiryDateException(int daysFromToday)
    {
        // Arrange
        var expiryDate = Today.AddDays(daysFromToday);

        // Act
        var ex = Assert.Throws<InvalidExpiryDateException>(() =>
            ProductBatch.Register(Guid.NewGuid(), 10, expiryDate, Today));

        // Assert
        Assert.Equal("batch.invalid_expiry_date", ex.Code);
        Assert.Equal(expiryDate, ex.ExpiryDate);
        Assert.Equal(Today, ex.Today);
    }

    // Reserve stock -> Stock reserved 

    [Fact]
    public void ReserveStock_WhenEnoughStock_ReducesAvailableQuantityAndAddsReservation()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);
        var orderId = Guid.NewGuid();

        // Act
        batch.ReserveStock(orderId, quantity: 30, Today);

        // Assert
        var reservation = Assert.Single(batch.Reservations);
        Assert.Equal(orderId, reservation.OrderId);
        Assert.Equal(30, reservation.Quantity);
        Assert.Equal(30, batch.ReservedQuantity);
        Assert.Equal(70, batch.AvailableQuantity);
    }

    [Fact]
    public void ReserveStock_WhenSameOrderReservesTwice_MergesIntoOneReservation()
    {
        // Arrange
        var batch = CreateBatch(quantity: 100);
        var orderId = Guid.NewGuid();
        batch.ReserveStock(orderId, 20, Today);

        // Act
        batch.ReserveStock(orderId, 15, Today);

        // Assert
        var reservation = Assert.Single(batch.Reservations);
        Assert.Equal(35, reservation.Quantity);
        Assert.Equal(65, batch.AvailableQuantity);
    }

    [Fact]
    public void ReserveStock_WhenQuantityExceedsAvailableStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var batch = CreateBatch(quantity: 10);
        batch.ReserveStock(Guid.NewGuid(), 8, Today);

        // Act
        var ex = Assert.Throws<InsufficientStockException>(() =>
            batch.ReserveStock(Guid.NewGuid(), 5, Today));

        // Assert
        Assert.Equal("batch.insufficient_stock", ex.Code);
        Assert.Equal(5, ex.RequestedQuantity);
        Assert.Equal(2, ex.AvailableQuantity);
        Assert.Equal(2, batch.AvailableQuantity); // state unchanged
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ReserveStock_WhenQuantityIsNotPositive_ThrowsInvalidQuantityException(int invalidQuantity)
    {
        // Arrange
        var batch = CreateBatch();

        // Act
        var ex = Assert.Throws<InvalidQuantityException>(() =>
            batch.ReserveStock(Guid.NewGuid(), invalidQuantity, Today));

        // Assert
        Assert.Equal(invalidQuantity, ex.AttemptedQuantity);
        Assert.Empty(batch.Reservations);
    }

    [Fact]
    public void ReserveStock_WhenBatchHasExpired_ThrowsBatchExpiredException()
    {
        // Arrange
        var expiryDate = Today.AddDays(3);
        var batch = CreateBatch(expiryDate: expiryDate);

        // Act
        var ex = Assert.Throws<BatchExpiredException>(() =>
            batch.ReserveStock(Guid.NewGuid(), 1, expiryDate.AddDays(1)));

        // Assert
        Assert.Equal("batch.expired", ex.Code);
        Assert.Equal(batch.Id, ex.BatchId);
        Assert.Equal(expiryDate, ex.ExpiryDate);
        Assert.Empty(batch.Reservations);
    }

    [Fact]
    public void ReserveStock_OnExpiryDate_IsStillAllowed()
    {
        // Arrange
        var expiryDate = Today.AddDays(3);
        var batch = CreateBatch(expiryDate: expiryDate);

        // Act
        batch.ReserveStock(Guid.NewGuid(), 1, expiryDate);

        // Assert
        Assert.Equal(1, batch.ReservedQuantity);
    }

    // Release reservation -> Stock reservation release

    [Fact]
    public void ReleaseReservation_WhenReservationExists_RestoresAvailableStock()
    {
        // Arrange
        var batch = CreateBatch(quantity: 50);
        var orderId = Guid.NewGuid();
        batch.ReserveStock(orderId, 20, Today);

        // Act
        batch.ReleaseReservation(orderId);

        // Assert
        Assert.Empty(batch.Reservations);
        Assert.Equal(50, batch.AvailableQuantity);
    }

    [Fact]
    public void ReleaseReservation_WhenOrderHasNoReservation_LeavesBatchUnchanged()
    {
        // Arrange
        var batch = CreateBatch(quantity: 50);
        batch.ReserveStock(Guid.NewGuid(), 10, Today);

        // Act
        batch.ReleaseReservation(Guid.NewGuid());

        // Assert
        Assert.Single(batch.Reservations);
        Assert.Equal(40, batch.AvailableQuantity);
    }
}