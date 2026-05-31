using UnitConnect.Domain.Common;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Domain.Aggregates.Listings;

/// <summary>
/// A marketplace listing posted by a resident to sell or offer services.
/// </summary>
public sealed class Listing : AggregateRoot
{
    public Guid ComplexId { get; private set; }
    public Guid SellerId { get; private set; }       // Resident.Id

    public string Title { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public ListingCategory Category { get; private set; }
    public ListingStatus Status { get; private set; }

    // Images stored as relative paths / blob keys — no binary in the domain
    private readonly List<ListingImage> _images = [];
    public IReadOnlyCollection<ListingImage> Images => _images.AsReadOnly();

    private readonly List<ListingContactRequest> _contactRequests = [];
    public IReadOnlyCollection<ListingContactRequest> ContactRequests => _contactRequests.AsReadOnly();

    // Required by EF Core
    private Listing()
    {
        Title = string.Empty; Description = string.Empty; Price = null!;
    }

    private Listing(
        Guid complexId, Guid sellerId,
        string title, string description,
        Money price, ListingCategory category)
    {
        ComplexId   = complexId;
        SellerId    = sellerId;
        Title       = title;
        Description = description;
        Price       = price;
        Category    = category;
        Status      = ListingStatus.Active;
    }

    public static Listing Create(
        Guid complexId, Guid sellerId,
        string title, string description,
        Money price, ListingCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Listing title is required.");

        if (title.Length > 120)
            throw new DomainException("Listing title cannot exceed 120 characters.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Listing description is required.");

        var listing = new Listing(
            complexId, sellerId,
            title.Trim(), description.Trim(),
            price, category);

        listing.RaiseDomainEvent(new ListingCreatedEvent(listing.Id, complexId, sellerId));
        return listing;
    }

    public void UpdateDetails(string title, string description, Money price, ListingCategory category)
    {
        if (Status != ListingStatus.Active)
            throw new DomainException("Only active listings can be edited.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Listing title is required.");

        Title       = title.Trim();
        Description = description.Trim();
        Price       = price;
        Category    = category;
        MarkUpdated();
    }

    public void AddImage(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException("Image storage path is required.");

        if (_images.Count >= 6)
            throw new DomainException("A listing may have at most 6 images.");

        _images.Add(ListingImage.Create(Id, storagePath, _images.Count));
        MarkUpdated();
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new DomainException("Image not found on this listing.");

        _images.Remove(image);
        MarkUpdated();
    }

    /// <summary>
    /// A buyer clicks "Contact Seller" — records interest without exposing contact details.
    /// </summary>
    public ListingContactRequest RequestContact(Guid buyerId)
    {
        if (Status != ListingStatus.Active)
            throw new DomainException("This listing is no longer active.");

        if (buyerId == SellerId)
            throw new DomainException("You cannot contact yourself.");

        if (_contactRequests.Any(r => r.BuyerId == buyerId))
            throw new DomainException("You have already requested contact for this listing.");

        var request = ListingContactRequest.Create(Id, buyerId);
        _contactRequests.Add(request);
        RaiseDomainEvent(new ListingContactRequestedEvent(Id, SellerId, buyerId));
        return request;
    }

    public void MarkAsSold()
    {
        if (Status != ListingStatus.Active)
            throw new DomainException("Listing is not active.");

        Status = ListingStatus.Sold;
        MarkUpdated();
        RaiseDomainEvent(new ListingSoldEvent(Id, ComplexId, SellerId));
    }

    public void Withdraw()
    {
        if (Status != ListingStatus.Active)
            throw new DomainException("Only active listings can be withdrawn.");

        Status = ListingStatus.Withdrawn;
        MarkUpdated();
    }
}
