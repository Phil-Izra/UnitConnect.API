using UnitConnect.Domain.Common;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;

namespace UnitConnect.Domain.Aggregates.Notices;

/// <summary>
/// A communication posted to all residents of a complex.
/// Urgency level drives whether a push notification is triggered.
/// </summary>
public sealed class Notice : AggregateRoot
{
    public Guid ComplexId { get; private set; }
    public Guid PostedByResidentId { get; private set; }

    public string Title { get; private set; }
    public string Body { get; private set; }
    public NoticeUrgency Urgency { get; private set; }
    public NoticeStatus Status { get; private set; }

    /// <summary>
    /// UTC time after which the notice is automatically considered stale (optional).
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    private readonly List<NoticeAcknowledgement> _acknowledgements = [];
    public IReadOnlyCollection<NoticeAcknowledgement> Acknowledgements => _acknowledgements.AsReadOnly();

    // Required by EF Core
    private Notice() { Title = string.Empty; Body = string.Empty; }

    private Notice(
        Guid complexId, Guid postedByResidentId,
        string title, string body,
        NoticeUrgency urgency, DateTime? expiresAt)
    {
        ComplexId           = complexId;
        PostedByResidentId  = postedByResidentId;
        Title               = title;
        Body                = body;
        Urgency             = urgency;
        Status              = NoticeStatus.Published;
        ExpiresAt           = expiresAt;
    }

    public static Notice Create(
        Guid complexId, Guid postedByResidentId,
        string title, string body,
        NoticeUrgency urgency = NoticeUrgency.General,
        DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Notice title is required.");

        if (title.Length > 200)
            throw new DomainException("Notice title cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Notice body is required.");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Expiry date must be in the future.");

        var notice = new Notice(complexId, postedByResidentId, title.Trim(), body.Trim(), urgency, expiresAt);
        notice.RaiseDomainEvent(new NoticePublishedEvent(notice.Id, complexId, urgency));
        return notice;
    }

    public void EditContent(string title, string body)
    {
        if (Status != NoticeStatus.Published)
            throw new DomainException("Only published notices can be edited.");

        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(body))  throw new DomainException("Body is required.");

        Title = title.Trim();
        Body  = body.Trim();
        MarkUpdated();
    }

    public void Archive()
    {
        if (Status == NoticeStatus.Archived)
            throw new DomainException("Notice is already archived.");

        Status = NoticeStatus.Archived;
        MarkUpdated();
    }

    /// <summary>
    /// Records that a resident has read/acknowledged this notice.
    /// Idempotent — duplicate acks are silently ignored.
    /// </summary>
    public void Acknowledge(Guid residentId)
    {
        if (_acknowledgements.Any(a => a.ResidentId == residentId))
            return;   // already acknowledged — no error needed

        _acknowledgements.Add(NoticeAcknowledgement.Create(Id, residentId));
    }

    public bool IsExpired() =>
        ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public bool RequiresPushNotification() =>
        Urgency == NoticeUrgency.Urgent;
}
