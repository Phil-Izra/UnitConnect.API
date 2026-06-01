using Microsoft.EntityFrameworkCore;
using UnitConnect.Domain.Aggregates.Complexes;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Infrastructure.Persistence.Repositories;

public sealed class ComplexRepository(AppDbContext context) : IComplexRepository
{
    public Task<Complex?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Complex>()
            .Include("_units")
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Complex>().AnyAsync(c => c.Id == id, ct);

    public async Task AddAsync(Complex complex, CancellationToken ct = default) =>
        await context.Set<Complex>().AddAsync(complex, ct);

    public Task SaveAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
