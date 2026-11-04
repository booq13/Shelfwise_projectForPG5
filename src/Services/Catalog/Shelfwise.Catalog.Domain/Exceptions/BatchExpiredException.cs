namespace Shelfwise.Catalog.Domain.Exceptions;

public sealed class BatchExpiredException : DomainException
{
    public Guid BatchId { get; }
    public DateOnly ExpiryDate { get; }

    public BatchExpiredException(Guid batchId, DateOnly expiryDate)
        : base(
            code: "batch.expired",
            message: $"Batch '{batchId}' expired on {expiryDate:yyyy-MM-dd} and cannot be reserved.")
    {
        BatchId = batchId;
        ExpiryDate = expiryDate;
    }
}