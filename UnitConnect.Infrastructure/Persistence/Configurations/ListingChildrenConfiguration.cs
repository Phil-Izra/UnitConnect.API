using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Listings;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("ListingImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ListingId).IsRequired();
        builder.Property(i => i.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(i => i.DisplayOrder).IsRequired();
    }
}

public sealed class ListingContactRequestConfiguration : IEntityTypeConfiguration<ListingContactRequest>
{
    public void Configure(EntityTypeBuilder<ListingContactRequest> builder)
    {
        builder.ToTable("ListingContactRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ListingId).IsRequired();
        builder.Property(r => r.BuyerId).IsRequired();
        builder.Property(r => r.IsViewed).IsRequired();

        builder.HasIndex(r => new { r.ListingId, r.BuyerId }).IsUnique();
    }
}
