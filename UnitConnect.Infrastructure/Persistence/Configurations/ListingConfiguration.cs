using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Listings;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ComplexId).IsRequired();
        builder.Property(l => l.SellerId).IsRequired();
        builder.Property(l => l.Title).IsRequired().HasMaxLength(120);
        builder.Property(l => l.Description).IsRequired();
        builder.Property(l => l.Category).IsRequired();
        builder.Property(l => l.Status).IsRequired();

        // Money value object — stored as two columns
        builder.OwnsOne(l => l.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasMany(l => l.Images)
            .WithOne()
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.ContactRequests)
            .WithOne()
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Images).HasField("_images");
        builder.Navigation(l => l.ContactRequests).HasField("_contactRequests");
    }
}
