using UnitConnect.Domain.Common;

namespace UnitConnect.Domain.Aggregates.Notices;

/// <summary>
/// Records that a specific resident has read a Notice.
/// Useful for trustees to confirm important notices were seen.
/// </summary>
public sealed class NoticeAcknowledgement : Entity
{
    public Guid NoticeId { get; private set; }
    public Guid ResidentId { get; private set; }
    public DateTime AcknowledgedAt { get; private set; }

    private NoticeAcknowledgement() { }

    private NoticeAcknowledgement(Guid noticeId, Guid residentId)
    {
        NoticeId        = noticeId;
        ResidentId      = residentId;
        AcknowledgedAt  = DateTime.UtcNow;
    }

    internal static NoticeAcknowledgement Create(Guid noticeId, Guid residentId) =>
        new(noticeId, residentId);
}
