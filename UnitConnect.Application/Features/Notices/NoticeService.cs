using UnitConnect.Application.Exceptions;
using UnitConnect.Domain.Aggregates.Notices;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Application.Features.Notices;

public sealed class NoticeService(INoticeRepository noticeRepository) : INoticeService
{
    public async Task<Guid> PublishAsync(
        Guid complexId, Guid postedByResidentId,
        PublishNoticeRequest request, CancellationToken ct = default)
    {
        var notice = Notice.Create(
            complexId, postedByResidentId,
            request.Title, request.Body,
            request.Urgency, request.ExpiresAt);

        await noticeRepository.AddAsync(notice, ct);
        await noticeRepository.SaveAsync(ct);
        return notice.Id;
    }

    public async Task EditContentAsync(Guid noticeId, EditNoticeRequest request, CancellationToken ct = default)
    {
        var notice = await noticeRepository.GetByIdAsync(noticeId, ct)
            ?? throw new NotFoundException($"Notice {noticeId} not found.");

        notice.EditContent(request.Title, request.Body);
        await noticeRepository.SaveAsync(ct);
    }

    public async Task AcknowledgeAsync(Guid noticeId, Guid residentId, CancellationToken ct = default)
    {
        var notice = await noticeRepository.GetByIdAsync(noticeId, ct)
            ?? throw new NotFoundException($"Notice {noticeId} not found.");

        notice.Acknowledge(residentId);
        await noticeRepository.SaveAsync(ct);
    }

    public async Task ArchiveAsync(Guid noticeId, CancellationToken ct = default)
    {
        var notice = await noticeRepository.GetByIdAsync(noticeId, ct)
            ?? throw new NotFoundException($"Notice {noticeId} not found.");

        notice.Archive();
        await noticeRepository.SaveAsync(ct);
    }

    public async Task<IReadOnlyList<NoticeResponse>> GetPublishedByComplexAsync(Guid complexId, CancellationToken ct = default)
    {
        var notices = await noticeRepository.GetPublishedByComplexAsync(complexId, ct);
        return notices.Select(ToResponse).ToList();
    }

    public async Task<NoticeResponse?> GetByIdAsync(Guid noticeId, CancellationToken ct = default)
    {
        var notice = await noticeRepository.GetByIdAsync(noticeId, ct);
        return notice is null ? null : ToResponse(notice);
    }

    private static NoticeResponse ToResponse(Notice n) => new(
        n.Id,
        n.ComplexId,
        n.PostedByResidentId,
        n.Title,
        n.Body,
        n.Urgency,
        n.Status,
        n.ExpiresAt,
        n.IsExpired(),
        n.Acknowledgements
            .Select(a => new NoticeAcknowledgementResponse(a.ResidentId, a.AcknowledgedAt))
            .ToList());
}
