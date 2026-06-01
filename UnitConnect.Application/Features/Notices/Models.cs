using UnitConnect.Domain.Enums;

namespace UnitConnect.Application.Features.Notices;

public record PublishNoticeRequest(
    string Title,
    string Body,
    NoticeUrgency Urgency = NoticeUrgency.General,
    DateTime? ExpiresAt = null);

public record EditNoticeRequest(
    string Title,
    string Body);

public record NoticeAcknowledgementResponse(
    Guid ResidentId,
    DateTime AcknowledgedAt);

public record NoticeResponse(
    Guid Id,
    Guid ComplexId,
    Guid PostedByResidentId,
    string Title,
    string Body,
    NoticeUrgency Urgency,
    NoticeStatus Status,
    DateTime? ExpiresAt,
    bool IsExpired,
    IReadOnlyList<NoticeAcknowledgementResponse> Acknowledgements);
