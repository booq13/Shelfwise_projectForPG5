namespace Shelfwise.Catalog.Domain.Exceptions;

public sealed class BatchNotFoundException : DomainException
{
    public Guid BatchId { get; }

    public BatchNotFoundException(Guid batchId)
        : base(
            code: "batch.not_found",
            message: $"Batch with ID '{batchId}' was not found.")
    {
        BatchId = batchId;
    }
}