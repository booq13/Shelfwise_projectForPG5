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

            // Own identity of the entity, created in C#
            reservation.HasKey(r => r.Id);
            reservation.Property(r => r.Id)
                .ValueGeneratedNever();

            reservation.Property(r => r.OrderId)
                .IsRequired();

            reservation.Property(r => r.Quantity)
                .IsRequired();

            reservation.Property(r => r.ReservedOn)
                .IsRequired();

            // Business rule kept in the database: one reservation per order in a batch
            reservation.HasIndex("BatchId", nameof(StockReservation.OrderId))
                .IsUnique();
        });

        builder.Navigation(b => b.Reservations)
            .HasField("_reservations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}