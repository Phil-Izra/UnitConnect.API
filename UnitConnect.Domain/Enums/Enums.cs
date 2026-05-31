namespace UnitConnect.Domain.Enums;

public enum ResidentStatus
{
    PendingInvite,   // invite sent, not yet registered
    Active,          // registered and active
    Suspended,       // access temporarily disabled by admin
    Removed          // no longer a resident
}

public enum ResidentRole
{
    Owner,           // unit owner
    Tenant,          // renting a unit
    Trustee,         // body corporate trustee
    Admin            // complex administrator (managing agent)
}

public enum ListingStatus
{
    Active,
    Sold,
    Withdrawn
}

public enum ListingCategory
{
    Furniture,
    Electronics,
    Appliances,
    Garden,
    Clothing,
    ChildrenItems,
    Vehicles,
    Services,      // internal services: cleaning, babysitting, handyman etc.
    Other
}

public enum NoticeUrgency
{
    General,        // normal notice board post
    Important,      // highlighted
    Urgent          // push notification triggered
}

public enum NoticeStatus
{
    Published,
    Archived
}

public enum InviteStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled
}
