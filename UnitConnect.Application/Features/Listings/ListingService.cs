using UnitConnect.Application.Exceptions;
using UnitConnect.Domain.Aggregates.Listings;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Application.Features.Listings;

public sealed class ListingService(IListingRepository listingRepository) : IListingService
{
    public async Task<Guid> CreateAsync(
        Guid complexId, Guid sellerId,
        CreateListingRequest request, CancellationToken ct = default)
    {
        var price = Money.Create(request.Price, request.Currency);
        var listing = Listing.Create(complexId, sellerId, request.Title, request.Description, price, request.Category);
        await listingRepository.AddAsync(listing, ct);
        await listingRepository.SaveAsync(ct);
        return listing.Id;
    }

    public async Task UpdateAsync(Guid listingId, UpdateListingRequest request, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        var price = Money.Create(request.Price, request.Currency);
        listing.UpdateDetails(request.Title, request.Description, price, request.Category);
        await listingRepository.SaveAsync(ct);
    }

    public async Task AddImageAsync(Guid listingId, string storagePath, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        listing.AddImage(storagePath);
        await listingRepository.SaveAsync(ct);
    }

    public async Task RemoveImageAsync(Guid listingId, Guid imageId, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        listing.RemoveImage(imageId);
        await listingRepository.SaveAsync(ct);
    }

    public async Task MarkAsSoldAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        listing.MarkAsSold();
        await listingRepository.SaveAsync(ct);
    }

    public async Task WithdrawAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        listing.Withdraw();
        await listingRepository.SaveAsync(ct);
    }

    public async Task<ListingContactRequestResponse> RequestContactAsync(
        Guid listingId, Guid buyerId, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct)
            ?? throw new NotFoundException($"Listing {listingId} not found.");

        var contactRequest = listing.RequestContact(buyerId);
        await listingRepository.SaveAsync(ct);
        return new ListingContactRequestResponse(contactRequest.Id, contactRequest.BuyerId, contactRequest.IsViewed);
    }

    public async Task<ListingResponse?> GetByIdAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingRepository.GetByIdAsync(listingId, ct);
        return listing is null ? null : ToResponse(listing);
    }

    public async Task<IReadOnlyList<ListingResponse>> GetActiveByComplexAsync(
        Guid complexId, ListingCategory? category = null, CancellationToken ct = default)
    {
        var listings = await listingRepository.GetActiveByComplexAsync(complexId, category, ct);
        return listings.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<ListingResponse>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default)
    {
        var listings = await listingRepository.GetBySellerAsync(sellerId, ct);
        return listings.Select(ToResponse).ToList();
    }

    private static ListingResponse ToResponse(Listing l) => new(
        l.Id,
        l.ComplexId,
        l.SellerId,
        l.Title,
        l.Description,
        l.Price.Amount,
        l.Price.Currency,
        l.Category,
        l.Status,
        l.Images.Select(i => new ListingImageResponse(i.Id, i.StoragePath, i.DisplayOrder)).ToList());
}
