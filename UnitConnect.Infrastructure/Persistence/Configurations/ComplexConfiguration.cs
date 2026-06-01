using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Complexes;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ComplexConfiguration : IEntityTypeConfiguration<Complex>
{
    public void Configure(EntityTypeBuilder<Complex> builder)
    {
        builder.ToTable("Complexes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.PhysicalAddress).IsRequired().HasMaxLength(500);
        builder.Property(c => c.AdminContactEmail).HasMaxLength(256);
        builder.Property(c => c.IsActive).IsRequired();

        builder.HasMany(c => c.Units)
            .WithOne()
            .HasForeignKey(u => u.ComplexId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Units).HasField("_units");
    }
}
