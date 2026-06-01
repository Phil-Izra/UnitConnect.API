using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.ToTable("Residents");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ComplexId).IsRequired();
        builder.Property(r => r.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.LastName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.PasswordHash).IsRequired();
        builder.Property(r => r.UnitNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Role).IsRequired();
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.PasswordResetToken).HasMaxLength(100);

        builder.Property(r => r.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("Email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(r => new { r.ComplexId, r.Email }).IsUnique();
    }
}
