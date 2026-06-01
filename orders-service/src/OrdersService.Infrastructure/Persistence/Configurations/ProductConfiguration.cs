using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Persistence.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.ProductCode);

        builder.Property(p => p.ProductCode)
            .HasColumnName("productCode")
            .IsRequired()
            .HasMaxLength(15);
        
        builder.Property(p => p.QuantityInStock)
            .HasColumnName("quantityInStock")
            .IsRequired();
        
        builder.Property(p => p.RetailPrice)
            .HasColumnName("MSRP")
            .HasPrecision(10, 2)
            .IsRequired();
    }
}