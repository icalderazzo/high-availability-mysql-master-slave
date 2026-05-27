using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("orderdetails");

        // Composite primary key matching classicmodels schema: (orderNumber, productCode)
        builder.HasKey(i => new { i.OrderNumber, i.ProductCode });

        builder.Property(i => i.OrderNumber)
            .HasColumnName("orderNumber")
            .IsRequired();

        builder.Property(i => i.ProductCode)
            .HasColumnName("productCode")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(i => i.QuantityOrdered)
            .HasColumnName("quantityOrdered")
            .IsRequired();

        builder.Property(i => i.PriceEach)
            .HasColumnName("priceEach")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.OrderLineNumber)
            .HasColumnName("orderLineNumber")
            .IsRequired();

        builder.Ignore(i => i.LineTotal);

        builder.HasIndex(i => i.OrderNumber)
            .HasDatabaseName("ix_orderdetails_order_number");
    }
}
