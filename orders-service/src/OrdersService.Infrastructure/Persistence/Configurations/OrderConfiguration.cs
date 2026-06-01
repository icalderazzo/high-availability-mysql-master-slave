using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Enums;

namespace OrdersService.Infrastructure.Persistence.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("orderNumber")
            .ValueGeneratedNever();

        builder.Property(o => o.CustomerNumber)
            .HasColumnName("customerNumber")
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .HasColumnName("orderDate")
            .IsRequired();

        builder.Property(o => o.RequiredDate)
            .HasColumnName("requiredDate")
            .IsRequired();

        builder.Property(o => o.ShippedDate)
            .HasColumnName("shippedDate");

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasMaxLength(15)
            .IsRequired()
            .HasConversion(
                s => ToDbString(s),
                s => FromDbString(s));

        builder.Property(o => o.Comments)
            .HasColumnName("comments")
            .HasColumnType("text");

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderNumber)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.CustomerNumber)
            .HasDatabaseName("ix_orders_customer_number");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("ix_orders_status");
    }

    private static string ToDbString(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.InProcess => "In Process",
            OrderStatus.OnHold => "On Hold",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Resolved => "Resolved",
            OrderStatus.Disputed => "Disputed",
            OrderStatus.Shipped => "Shipped",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static OrderStatus FromDbString(string status)
    {
        return status switch
        {
            "In Process" => OrderStatus.InProcess,
            "On Hold" => OrderStatus.OnHold,
            "Cancelled" => OrderStatus.Cancelled,
            "Resolved" => OrderStatus.Resolved,
            "Disputed" => OrderStatus.Disputed,
            "Shipped" => OrderStatus.Shipped,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
