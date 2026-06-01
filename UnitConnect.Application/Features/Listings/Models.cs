using UnitConnect.Domain.Enums;

namespace UnitConnect.Application.Features.Listings;

public record CreateListingRequest(
    string Title,
    string Description,
    decimal Price,
    string Currency,
    ListingCategory Category);

public record UpdateListingRequest(
    string Title,
    string Description,
    decimal Price,
    string Currency,
    ListingCategory Category);

public record ListingImageResponse(
    Guid Id,
    string StoragePath,
    int DisplayOrder);

public record ListingContactRequestResponse(
    Guid Id,
    Guid BuyerId,
    bool IsViewed);

public record ListingResponse(
    Guid Id,
    Guid ComplexId,
    Guid SellerId,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    ListingCategory Category,
    ListingStatus Status,
    IReadOnlyList<ListingImageResponse> Images);
