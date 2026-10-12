# Shelfwise

An e-commerce backend for shops that sell products with an expiry date: food, cosmetics, supplements, pet food.

## The problem

Shops throw away a lot of products simply because they did not sell in time. Discounts are usually set by hand and too late, often only on the last day, when it is already hard to sell the item.

## The idea

Shelfwise tracks every delivered batch of a product together with its expiry date. Each day it checks how fast a product is selling and estimates whether the batch will sell out before it expires. If not, the price is lowered step by step, early enough to make a difference. When a customer places an order, the items that expire first are always taken first (FEFO).

The goal is simple: less waste, more revenue.

To check that it really works, the project includes a simulator that runs the same shop for 90 days with three strategies:

1. no discounts,
2. a fixed discount in the last days (what most shops do today),
3. Shelfwise dynamic pricing,

and compares waste, revenue and margin.

## Domains

- **Customers** - registration, addresses, favourite products
- **Products** - catalog, batches with expiry dates, stock
- **Orders** - cart, ordering, stock reservation (FEFO)
- **Payments** - payment processing through an external (simulated) payment gateway
- **Invoices / Receipts** - document generated after a successful payment
- **Pricing** - dynamic discounts based on expiry date and sales speed

## How the course requirements are covered

| Course topic | In this project |
|---|---|
| .NET environment and local infrastructure | .NET, PostgreSQL, Kafka, Jaeger in Docker Compose |
| Business logic | Orders, stock reservation, pricing rules |
| Hexagonal architecture | Domain logic separated from database and API (ports and adapters) |
| Physical service extraction | Separate services: Catalog, Customers, Ordering, Payments, Invoicing, Pricing |
| HTTP communication, exceptions, tracing | Ordering calls Catalog over HTTP, global error handling, OpenTelemetry + Jaeger |
| Clean Architecture | Domain / Application / Infrastructure / API layers in every service |
| Circuit Breaker and Retry | Polly on calls to the payment gateway and between services |
| In-process events | Domain events inside services (e.g. batch close to expiry) |
| Kafka producer and consumer | Order flow: order placed, stock reserved, payment done, invoice issued |
| Outbox pattern | Events are saved in the same transaction as the data and published later |
| Tests | Unit tests (xUnit), integration tests (Testcontainers) |

## Order flow

```
Ordering --OrderPlaced--> Catalog (reserve stock, FEFO)
Catalog --StockReserved--> Payments (charge via gateway)
Payments --PaymentSucceeded--> Ordering (confirm order)
Ordering --OrderConfirmed--> Invoicing (generate invoice / receipt)
Pricing --PriceChanged--> Catalog (update current price)
```

## Tech stack

.NET, ASP.NET Core, Entity Framework Core, PostgreSQL, Apache Kafka, Polly, OpenTelemetry, Jaeger, xUnit, Testcontainers, Docker Compose.

## Running locally

```
docker compose up -d
```

Swagger for each service is available after start. Details will be added as the project grows.

## Authors

Denys Romaniuk