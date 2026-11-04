namespace Shelfwise.Catalog.Domain.Exceptions;

public sealed class InvalidExpiryDateException : DomainException
{
    public DateOnly ExpiryDate { get; }
    public DateOnly Today { get; }

    public InvalidExpiryDateException(DateOnly expiryDate, DateOnly today)
        : base(
            code: "batch.invalid_expiry_date",
            message: $"Expiry date ({expiryDate:yyyy-MM-dd}) must be later than today ({today:yyyy-MM-dd}).")
    {
        ExpiryDate = expiryDate;
        Today = today;
    }
}