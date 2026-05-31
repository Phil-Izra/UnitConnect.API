using UnitConnect.Domain.Common;

namespace UnitConnect.Domain.Aggregates.Listings;

/// <summary>
/// An image attached to a Listing. Stores only the storage key/path.
/// </summary>
public sealed class ListingImage : Entity
{
    public Guid ListingId { get; private set; }
    public string StoragePath { get; private set; }
    public int DisplayOrder { get; private set; }

    private ListingImage() { StoragePath = string.Empty; }

    private ListingImage(Guid listingId, string storagePath, int displayOrder)
    {
        ListingId    = listingId;
        StoragePath  = storagePath;
        DisplayOrder = displayOrder;
    }

    internal static ListingImage Create(Guid listingId, string storagePath, int displayOrder) =>
        new(listingId, storagePath, displayOrder);
}

/// <summary>
/// Records that a buyer has expressed interest and wants the seller's contact details.
/// The application layer resolves the actual contact info when this is read.
/// </summary>
public sealed class ListingContactRequest : Entity
{
    public Guid ListingId { get; private set; }
    public Guid BuyerId { get; private set; }
    public bool IsViewed { get; private set; }   // seller has seen this request

    private ListingContactRequest() { }

    private ListingContactRequest(Guid listingId, Guid buyerId)
    {
        ListingId = listingId;
        BuyerId   = buyerId;
        IsViewed  = false;
    }

    internal static ListingContactRequest Create(Guid listingId, Guid buyerId) =>
        new(listingId, buyerId);

    public void MarkViewed()
    {
        IsViewed = true;
        MarkUpdated();
    }
}
