# Aggregates and Invariants – Shelfwise

## 1. Value Objects

In the Lab 3 code, business concepts are still stored as raw types:

```csharp
ReserveStockAsync(Guid batchId, Guid orderId, int quantity)
ProductBatch.Register(Guid productId, int quantity, DateOnly expiryDate, DateOnly today)
```

The compiler does not stop `ReserveStockAsync(orderId, batchId, -3)`: both ids are `Guid`, and `int` accepts negative numbers. The quantity check is also repeated in three places (`Register`, `ReserveStock`, `StockReservation`). The Value Objects below remove this Primitive Obsession. They are the target design for the next refactoring of the Catalog and Pricing services.

| Value Object | Replaces | Attributes | Self-validation (rejected at creation) | Operation returning a new instance | Event Storming board |
|---|---|---|---|---|---|
| `Quantity` | `int` | `int Value` | `Value <= 0` → `InvalidQuantityException`. No upper limit: a shop may sell or buy any number of items. | `Add(Quantity other)`: merges two reservations of the same order (4 + 4 = 8). | Batch delivered, Reserve stock, Stock reserved |
| `ExpiryDate` | `DateOnly` | `DateOnly Value` | Empty date (`DateOnly.MinValue`) → `InvalidExpiryDateException`. | `CorrectTo(DateOnly newDate)`: replaces a wrongly recorded expiry date with a new object. Query: `IsExpiredOn(DateOnly date)`. | Record expiry date, Batch expiry date recorded, Batch expired, Batches near expiry |
| `Money` | `decimal` | `decimal Amount`, `string Currency` | `Amount < 0` → `InvalidMoneyAmountException`; currency other than `PLN` or `EUR` → `InvalidCurrencyException`; adding different currencies → `CurrencyMismatchException`. The amount is rounded to 2 decimal places. Zero is allowed (e.g. a discount amount when there is no discount). | `ApplyDiscount(DiscountRate rate)`: new price after a discount (10.00 PLN − 30% = 7.00 PLN). `Add(Money other)`: sum of two amounts in the same currency. | Product price set, Price changed, Current price updated, Amount to pay, Invoice issued |
| `DiscountRate` | `decimal` (%) | `decimal Percentage` | `Percentage < 0` or `Percentage >= 100` → `InvalidDiscountRateException`. | `IncreaseBy(DiscountRate step)`: next discount step (10% → 20%). | Batch marked as at risk, Change price, Price changed, Discount steps and minimum margin |

### Self-validation vs. aggregate invariants

A Value Object checks only rules that are true **always**, whatever the situation. Rules that depend on context belong to the aggregate:

| Rule | Where it lives | Why |
|---|---|---|
| The expiry date is not empty | `ExpiryDate` | Always true. |
| The expiry date is later than today at delivery | `ProductBatch.Register` | Depends on the moment of registration. If `ExpiryDate` checked it, a batch that expired yesterday could not even be loaded, so it could not be shown in "Batches near expiry" or written off. |
| Money is never negative | `Money` | Always true. |
| A product price is greater than zero | `Product` / `Batch Price` | Some amounts are legitimately zero (no discount, empty cart), so `Money` must allow zero. |
| A discount is below 100% | `DiscountRate` | Shelfwise never gives products away: unsold batches are written off (Batch expired → Write off batch). |
| The price after a discount is not below cost + minimum margin | `Batch Price` | Depends on the cost of a specific batch. Violation → Price change rejected (below minimum margin). |

### Currency

`Money` keeps the currency because Shelfwise is a backend for many shops, in Poland and in the euro zone, and suppliers often price goods in EUR. Keeping the currency makes it impossible to add EUR to PLN by mistake.

## 2. Strongly typed identifiers

Each aggregate root has its own id type instead of a raw `Guid`, so two different ids can never be swapped:

```csharp
// Today: compiles even if batchId and orderId are swapped
ReserveStockAsync(Guid batchId, Guid orderId, int quantity)

// Target: swapping the arguments does not compile
ReserveStockAsync(BatchId batchId, OrderId orderId, Quantity quantity)
```

| Identifier | Wraps | Aggregate | Bounded context |
|---|---|---|---|
| `ProductId` | `Guid` | Product | Catalog |
| `BatchId` | `Guid` | Product Batch | Catalog |
| `CustomerId` | `Guid` | Customer | Customers |
| `CartId` | `Guid` | Cart | Ordering |
| `OrderId` | `Guid` | Order | Ordering |
| `PaymentId` | `Guid` | Payment | Payments |
| `InvoiceId` | `Guid` | Invoice | Invoicing |

The Batch Price aggregate in Pricing prices one batch, so it is identified by the `BatchId` of that batch. Other aggregates refer to it only by this id, never by an object reference.
