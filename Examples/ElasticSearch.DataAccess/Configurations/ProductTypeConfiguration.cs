using ElasticSearch.Models.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElasticSearch.DataAccess.Configurations
{
    public class ProductTypeConfiguration : BasicEntityTypeConfiguration<ProductDbModel>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProductDbModel> builder)
        {
            builder.ToTable("Products");

            builder.Property(p => p.ProductName)
                   .HasColumnName("ProductName")
                   .IsRequired();

            builder.Property(p => p.SupplierId)
                   .HasColumnName("SupplierID")
                   .IsRequired();

            builder.Property(p => p.CategoryId)
                   .HasColumnName("CategoryID")
                   .IsRequired();

            builder.Property(p => p.QuantityPerUnit)
                   .HasColumnName("QuantityPerUnit")
                   .IsRequired();

            builder.Property(p => p.UnitPrice)
                   .HasColumnName("UnitPrice")
                   .IsRequired();

            builder.Property(p => p.UnitsInStock)
                   .HasColumnName("UnitsInStock")
                   .IsRequired();

            builder.Property(p => p.UnitsOnOrder)
                   .HasColumnName("UnitsOnOrder")
                   .IsRequired();

            builder.Property(p => p.ReorderLevel)
                   .HasColumnName("ReorderLevel")
                   .IsRequired();

            builder.Property(p => p.Discontinued)
                   .HasColumnName("Discontinued")
                   .IsRequired();
        }
    }
}
