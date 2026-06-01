using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ResidentInviteConfiguration : IEntityTypeConfiguration<ResidentInvite>
{
    public void Configure(EntityTypeBuilder<ResidentInvite> builder)
    {
        builder.ToTable("ResidentInvites");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ComplexId).IsRequired();
        builder.Property(i => i.UnitNumber).IsRequired().HasMaxLength(20);
        builder.Property(i => i.IntendedRole).IsRequired();
        builder.Property(i => i.Token).IsRequired().HasMaxLength(100);
        builder.Property(i => i.ExpiresAt).IsRequired();
        builder.Property(i => i.Status).IsRequired();
        builder.Property(i => i.InvitedByResidentId).IsRequired();

        builder.Property(i => i.InvitedEmail)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("InvitedEmail")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(i => i.Token).IsUnique();
    }
}
