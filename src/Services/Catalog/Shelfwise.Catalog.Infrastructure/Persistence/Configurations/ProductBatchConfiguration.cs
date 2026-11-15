using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shelfwise.Catalog.Domain.Aggregates;
using Shelfwise.Catalog.Domain.Entities;

namespace Shelfwise.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductBatchConfiguration : IEntityTypeConfiguration<ProductBatch>
{
    public void Configure(EntityTypeBuilder<ProductBatch> builder)
    {
        builder.ToTable("ProductBatches", schema: "catalog");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.ProductId)
            .IsRequired();

        builder.HasIndex(b => b.ProductId);

        builder.Property(b => b.ExpiryDate)
            .IsRequired();

        builder.Property(b => b.DeliveredQuantity)
            .IsRequired();

        builder.Ignore(b => b.ReservedQuantity);
        builder.Ignore(b => b.AvailableQuantity);

        // Reservations are part of the aggregate: own table, loaded and saved together with the batch
        builder.OwnsMany(b => b.Reservations, reservation =>
        {
            reservation.ToTable("StockReservations", schema: "catalog");

            reservation.WithOwner().HasForeignKey("BatchId");

            reservation.HasKey("BatchId", nameof(StockReservation.OrderId));

            reservation.Property(r => r.OrderId)
                .ValueGeneratedNever();

            reservation.Property(r => r.Quantity)
                .IsRequired();
        });

        builder.Navigation(b => b.Reservations)
            .HasField("_reservations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}