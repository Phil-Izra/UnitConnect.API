using UnitConnect.Domain.Enums;

namespace UnitConnect.Application.Features.Listings;

public interface IListingService
{
    Task<Guid> CreateAsync(Guid complexId, Guid sellerId, CreateListingRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid listingId, UpdateListingRequest request, CancellationToken ct = default);
    Task AddImageAsync(Guid listingId, string storagePath, CancellationToken ct = default);
    Task RemoveImageAsync(Guid listingId, Guid imageId, CancellationToken ct = default);
    Task MarkAsSoldAsync(Guid listingId, CancellationToken ct = default);
    Task WithdrawAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingContactRequestResponse> RequestContactAsync(Guid listingId, Guid buyerId, CancellationToken ct = default);
    Task<ListingResponse?> GetByIdAsync(Guid listingId, CancellationToken ct = default);
    Task<IReadOnlyList<ListingResponse>> GetActiveByComplexAsync(Guid complexId, ListingCategory? category = null, CancellationToken ct = default);
    Task<IReadOnlyList<ListingResponse>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
}
