using Moq;
using Shelfwise.Catalog.Application.Abstractions;
using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Domain.Aggregates;
using Shelfwise.Catalog.Domain.Exceptions;
using Xunit;

namespace Shelfwise.Catalog.Application.Tests;

public class BatchServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 4);

    private readonly Mock<IBatchRepository> _repositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly BatchService _sut; // SUT = System Under Test

    public BatchServiceTests()
    {
        _clockMock.Setup(c => c.Today).Returns(Today);
        _sut = new BatchService(_repositoryMock.Object, _clockMock.Object);
    }

    private ProductBatch GivenExistingBatch(int quantity = 100)
    {
        var batch = ProductBatch.Register(Guid.NewGuid(), quantity, Today.AddDays(10), Today);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(batch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);
        return batch;
    }

    private void VerifyNeverSaved() =>
        _repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<ProductBatch>(), It.IsAny<CancellationToken>()),
            Times.Never);

    // Register batch

    [Fact]
    public async Task RegisterBatchAsync_WhenValidData_SavesBatchAndReturnsDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expiryDate = Today.AddDays(7);

        // Act
        var result = await _sut.RegisterBatchAsync(productId, 40, expiryDate);

        // Assert
        Assert.NotEqual(Guid.Empty, result.BatchId);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(expiryDate, result.ExpiryDate);
        Assert.Equal(40, result.AvailableQuantity);
        _repositoryMock.Verify(
            r => r.SaveAsync(
                It.Is<ProductBatch>(b => b.ProductId == productId && b.DeliveredQuantity == 40),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterBatchAsync_WhenExpiryDateInThePast_ThrowsAndNeverSaves()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidExpiryDateException>(
            () => _sut.RegisterBatchAsync(Guid.NewGuid(), 10, Today.AddDays(-1)));

        VerifyNeverSaved();
    }

    // Reserve stock

    [Fact]
    public async Task ReserveStockAsync_WhenBatchExists_ReservesStockAndSaves()
    {
        // Arrange
        var batch = GivenExistingBatch(quantity: 100);
        var orderId = Guid.NewGuid();

        // Act
        var result = await _sut.ReserveStockAsync(batch.Id, orderId, 25);

        // Assert
        Assert.Equal(25, result.ReservedQuantity);
        Assert.Equal(75, result.AvailableQuantity);
        var reservation = Assert.Single(result.Reservations);
        Assert.Equal(orderId, reservation.OrderId);
        _repositoryMock.Verify(r => r.SaveAsync(batch, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReserveStockAsync_WhenBatchDoesNotExist_ThrowsBatchNotFoundAndNeverSaves()
    {
        // Arrange
        var missingBatchId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(missingBatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductBatch?)null);

        // Act
        var ex = await Assert.ThrowsAsync<BatchNotFoundException>(
            () => _sut.ReserveStockAsync(missingBatchId, Guid.NewGuid(), 5));

        // Assert
        Assert.Equal("batch.not_found", ex.Code);
        Assert.Equal(missingBatchId, ex.BatchId);
        VerifyNeverSaved();
    }

    [Fact]
    public async Task ReserveStockAsync_WhenInsufficientStock_PropagatesExceptionAndNeverSaves()
    {
        // Arrange
        var batch = GivenExistingBatch(quantity: 3);

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(
            () => _sut.ReserveStockAsync(batch.Id, Guid.NewGuid(), 10));

        VerifyNeverSaved();
    }

    [Fact]
    public async Task ReserveStockAsync_UsesTodayFromClock_SoExpiredBatchIsRejected()
    {
        // Arrange: batch expires in 10 days, clock says it is day 11
        var batch = GivenExistingBatch();
        _clockMock.Setup(c => c.Today).Returns(Today.AddDays(11));

        // Act & Assert
        await Assert.ThrowsAsync<BatchExpiredException>(
            () => _sut.ReserveStockAsync(batch.Id, Guid.NewGuid(), 1));

        VerifyNeverSaved();
    }

    // Query: aavailable stock per batch 

    [Fact]
    public async Task GetBatchAsync_WhenBatchDoesNotExist_ReturnsNull()
    {
        // Arrange
        var missingBatchId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(missingBatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductBatch?)null);

        // Act
        var result = await _sut.GetBatchAsync(missingBatchId);

        // Assert
        Assert.Null(result);
    }

    // Query: list of batches

    [Fact]
    public async Task GetBatchesAsync_ReturnsAllBatchesAsDtos()
    {
        // Arrange
        var first = ProductBatch.Register(Guid.NewGuid(), 10, Today.AddDays(5), Today);
        var second = ProductBatch.Register(Guid.NewGuid(), 20, Today.AddDays(9), Today);
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductBatch> { first, second });

        // Act
        var result = await _sut.GetBatchesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.BatchId == first.Id && b.AvailableQuantity == 10);
        Assert.Contains(result, b => b.BatchId == second.Id && b.AvailableQuantity == 20);
    }

    // Command: delete batch

    [Fact]
    public async Task DeleteBatchAsync_WhenBatchExists_DeletesIt()
    {
        // Arrange
        var batch = GivenExistingBatch();

        // Act
        await _sut.DeleteBatchAsync(batch.Id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(batch, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBatchAsync_WhenBatchDoesNotExist_ThrowsBatchNotFoundAndNeverDeletes()
    {
        // Arrange
        var missingBatchId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(missingBatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductBatch?)null);

        // Act
        var ex = await Assert.ThrowsAsync<BatchNotFoundException>(
            () => _sut.DeleteBatchAsync(missingBatchId));

        // Assert
        Assert.Equal(missingBatchId, ex.BatchId);
        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<ProductBatch>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}