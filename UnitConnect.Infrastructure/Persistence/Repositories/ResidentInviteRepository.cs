using Microsoft.EntityFrameworkCore;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Infrastructure.Persistence.Repositories;

public sealed class ResidentInviteRepository(AppDbContext context) : IResidentInviteRepository
{
    public Task<ResidentInvite?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        context.Set<ResidentInvite>().FirstOrDefaultAsync(i => i.Token == token, ct);

    public Task<ResidentInvite?> GetPendingByEmailAndComplexAsync(string email, Guid complexId, CancellationToken ct = default) =>
        context.Set<ResidentInvite>()
            .FirstOrDefaultAsync(i =>
                i.InvitedEmail.Value == email &&
                i.ComplexId == complexId &&
                i.Status == InviteStatus.Pending, ct);

    public async Task AddAsync(ResidentInvite invite, CancellationToken ct = default) =>
        await context.Set<ResidentInvite>().AddAsync(invite, ct);

    public Task SaveAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
