using Microsoft.EntityFrameworkCore;
using UnitConnect.Domain.Aggregates.Listings;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository(AppDbContext context) : IListingRepository
{
    public Task<Listing?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Listing>()
            .Include("_images")
            .Include("_contactRequests")
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<Listing>> GetActiveByComplexAsync(
        Guid complexId, ListingCategory? category = null, CancellationToken ct = default)
    {
        var query = context.Set<Listing>()
            .Include("_images")
            .Where(l => l.ComplexId == complexId && l.Status == ListingStatus.Active);

        if (category.HasValue)
            query = query.Where(l => l.Category == category.Value);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Listing>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default) =>
        await context.Set<Listing>()
            .Include("_images")
            .Where(l => l.SellerId == sellerId)
            .ToListAsync(ct);

    public async Task AddAsync(Listing listing, CancellationToken ct = default) =>
        await context.Set<Listing>().AddAsync(listing, ct);

    public Task SaveAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
