using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Complexes;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class ComplexUnitConfiguration : IEntityTypeConfiguration<ComplexUnit>
{
    public void Configure(EntityTypeBuilder<ComplexUnit> builder)
    {
        builder.ToTable("ComplexUnits");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.ComplexId).IsRequired();
        builder.Property(u => u.Number).IsRequired().HasMaxLength(20);
        builder.Property(u => u.IsOccupied).IsRequired();

        builder.HasIndex(u => new { u.ComplexId, u.Number }).IsUnique();
    }
}
