using ElasticSearch.Models.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElasticSearch.DataAccess.Configurations
{
    public class SupplierTypeConfiguration : BasicEntityTypeConfiguration<SupplierDbModel>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SupplierDbModel> builder)
        {
            builder.ToTable("Suppliers");

            builder.Property(p => p.CompanyName)
                   .HasColumnName("CompanyName")
                   .IsRequired();

            builder.Property(p => p.ContactName)
                   .HasColumnName("ContactName");

            builder.Property(p => p.ContactTitle)
                   .HasColumnName("ContactTitle");

            builder.Property(p => p.Address)
                   .HasColumnName("Address");

            builder.Property(p => p.City)
                   .HasColumnName("City");

            builder.Property(p => p.Region)
                   .HasColumnName("Region");

            builder.Property(p => p.PostalCode)
                   .HasColumnName("PostalCode");

            builder.Property(p => p.Country)
                   .HasColumnName("Country");

            builder.Property(p => p.Phone)
                   .HasColumnName("Phone");

            builder.Property(p => p.Fax)
                   .HasColumnName("Fax");

            builder.Property(p => p.HomePage)
                   .HasColumnName("HomePage");

            builder.HasMany(p => p.Products)
                   .WithOne(p => p.Supplier)
                   .HasForeignKey(fk => fk.SupplierId);

        }
    }
}
