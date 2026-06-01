using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Notices;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.ToTable("Notices");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.ComplexId).IsRequired();
        builder.Property(n => n.PostedByResidentId).IsRequired();
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired();
        builder.Property(n => n.Urgency).IsRequired();
        builder.Property(n => n.Status).IsRequired();

        builder.HasMany(n => n.Acknowledgements)
            .WithOne()
            .HasForeignKey(a => a.NoticeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(n => n.Acknowledgements).HasField("_acknowledgements");
    }
}
