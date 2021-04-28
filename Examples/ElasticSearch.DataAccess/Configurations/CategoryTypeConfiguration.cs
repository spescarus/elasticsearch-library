using ElasticSearch.Models.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElasticSearch.DataAccess.Configurations
{
    public class CategoryTypeConfiguration : BasicEntityTypeConfiguration<CategoryDbModel>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CategoryDbModel> builder)
        {
            builder.ToTable("Categories");

            builder.Property(p => p.CategoryName)
                   .HasColumnName("CategoryName")
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasColumnName("Description");

            builder.Property(p => p.Picture)
                   .HasColumnName("Picture");

            builder.HasMany(p => p.Products)
                   .WithOne(p => p.Category)
                   .HasForeignKey(fk => fk.CategoryId);

        }
    }
}
