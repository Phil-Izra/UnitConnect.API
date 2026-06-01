using Microsoft.EntityFrameworkCore;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Infrastructure.Persistence.Repositories;

public sealed class ResidentRepository(AppDbContext context) : IResidentRepository
{
    public Task<Resident?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Resident>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Resident?> GetByEmailAsync(string email, Guid complexId, CancellationToken ct = default) =>
        context.Set<Resident>()
            .FirstOrDefaultAsync(r => r.Email.Value == email && r.ComplexId == complexId, ct);

    public Task<Resident?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default) =>
        context.Set<Resident>()
            .FirstOrDefaultAsync(r => r.PasswordResetToken == token, ct);

    public async Task<IReadOnlyList<Resident>> GetByComplexAsync(Guid complexId, CancellationToken ct = default) =>
        await context.Set<Resident>()
            .Where(r => r.ComplexId == complexId)
            .ToListAsync(ct);

    public Task<bool> EmailExistsInComplexAsync(string email, Guid complexId, CancellationToken ct = default) =>
        context.Set<Resident>()
            .AnyAsync(r => r.Email.Value == email && r.ComplexId == complexId, ct);

    public async Task AddAsync(Resident resident, CancellationToken ct = default) =>
        await context.Set<Resident>().AddAsync(resident, ct);

    public Task SaveAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
