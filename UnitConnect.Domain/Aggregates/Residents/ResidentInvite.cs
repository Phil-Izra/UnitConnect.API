using UnitConnect.Domain.Common;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Domain.Aggregates.Residents;

/// <summary>
/// An invite sent to a prospective resident.
/// Created by Admin/Trustee; consumed when the resident registers.
/// Kept as a separate aggregate so the app layer can query pending invites.
/// </summary>
public sealed class ResidentInvite : AggregateRoot
{
    public Guid ComplexId { get; private set; }
    public Email InvitedEmail { get; private set; }
    public string UnitNumber { get; private set; }
    public ResidentRole IntendedRole { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public InviteStatus Status { get; private set; }
    public Guid InvitedByResidentId { get; private set; }

    private ResidentInvite() { InvitedEmail = null!; UnitNumber = string.Empty; Token = string.Empty; }

    private ResidentInvite(
        Guid complexId, Email invitedEmail, string unitNumber,
        ResidentRole intendedRole, Guid invitedBy)
    {
        ComplexId         = complexId;
        InvitedEmail      = invitedEmail;
        UnitNumber        = unitNumber.ToUpperInvariant();
        IntendedRole      = intendedRole;
        Token             = Guid.NewGuid().ToString("N");
        ExpiresAt         = DateTime.UtcNow.AddDays(7);
        Status            = InviteStatus.Pending;
        InvitedByResidentId = invitedBy;
    }

    public static ResidentInvite Create(
        Guid complexId, Email invitedEmail,
        string unitNumber, ResidentRole intendedRole, Guid invitedBy)
    {
        if (string.IsNullOrWhiteSpace(unitNumber))
            throw new DomainException("Unit number is required for an invite.");

        return new ResidentInvite(complexId, invitedEmail, unitNumber, intendedRole, invitedBy);
    }

    public void Accept()
    {
        EnsurePendingAndValid();
        Status = InviteStatus.Accepted;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status != InviteStatus.Pending)
            throw new DomainException("Only pending invites can be cancelled.");

        Status = InviteStatus.Cancelled;
        MarkUpdated();
    }

    public bool IsValid() =>
        Status == InviteStatus.Pending && DateTime.UtcNow <= ExpiresAt;

    private void EnsurePendingAndValid()
    {
        if (Status != InviteStatus.Pending)
            throw new DomainException("This invite has already been used or cancelled.");

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = InviteStatus.Expired;
            throw new DomainException("This invite has expired. Please request a new one.");
        }
    }
}
