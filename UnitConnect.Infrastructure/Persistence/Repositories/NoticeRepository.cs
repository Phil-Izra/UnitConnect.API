using Microsoft.EntityFrameworkCore;
using UnitConnect.Domain.Aggregates.Notices;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Infrastructure.Persistence.Repositories;

public sealed class NoticeRepository(AppDbContext context) : INoticeRepository
{
    public Task<Notice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Notice>()
            .Include("_acknowledgements")
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<IReadOnlyList<Notice>> GetPublishedByComplexAsync(Guid complexId, CancellationToken ct = default) =>
        await context.Set<Notice>()
            .Include("_acknowledgements")
            .Where(n => n.ComplexId == complexId && n.Status == NoticeStatus.Published)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Notice>> GetUrgentUnacknowledgedAsync(
        Guid complexId, Guid residentId, CancellationToken ct = default) =>
        await context.Set<Notice>()
            .Include("_acknowledgements")
            .Where(n =>
                n.ComplexId == complexId &&
                n.Status == NoticeStatus.Published &&
                n.Urgency == NoticeUrgency.Urgent &&
                !n.Acknowledgements.Any(a => a.ResidentId == residentId))
            .ToListAsync(ct);

    public async Task AddAsync(Notice notice, CancellationToken ct = default) =>
        await context.Set<Notice>().AddAsync(notice, ct);

    public Task SaveAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
