using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitConnect.Domain.Aggregates.Notices;

namespace UnitConnect.Infrastructure.Persistence.Configurations;

public sealed class NoticeAcknowledgementConfiguration : IEntityTypeConfiguration<NoticeAcknowledgement>
{
    public void Configure(EntityTypeBuilder<NoticeAcknowledgement> builder)
    {
        builder.ToTable("NoticeAcknowledgements");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.NoticeId).IsRequired();
        builder.Property(a => a.ResidentId).IsRequired();
        builder.Property(a => a.AcknowledgedAt).IsRequired();

        builder.HasIndex(a => new { a.NoticeId, a.ResidentId }).IsUnique();
    }
}
