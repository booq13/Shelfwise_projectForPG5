namespace Shelfwise.Catalog.Domain.Exceptions;

public sealed class InvalidQuantityException : DomainException
{
    public int AttemptedQuantity { get; }

    public InvalidQuantityException(int attemptedQuantity)
        : base(
            code: "batch.invalid_quantity",
            message: $"Quantity must be greater than zero, but was {attemptedQuantity}.")
    {
        AttemptedQuantity = attemptedQuantity;
    }
}