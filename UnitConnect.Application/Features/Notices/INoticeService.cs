namespace UnitConnect.Application.Features.Notices;

public interface INoticeService
{
    Task<Guid> PublishAsync(Guid complexId, Guid postedByResidentId, PublishNoticeRequest request, CancellationToken ct = default);
    Task EditContentAsync(Guid noticeId, EditNoticeRequest request, CancellationToken ct = default);
    Task AcknowledgeAsync(Guid noticeId, Guid residentId, CancellationToken ct = default);
    Task ArchiveAsync(Guid noticeId, CancellationToken ct = default);
    Task<IReadOnlyList<NoticeResponse>> GetPublishedByComplexAsync(Guid complexId, CancellationToken ct = default);
    Task<NoticeResponse?> GetByIdAsync(Guid noticeId, CancellationToken ct = default);
}
