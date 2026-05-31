using UnitConnect.Domain.Common;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Domain.Aggregates.Residents;

/// <summary>
/// A registered dweller of a complex.
/// Owns identity (email/password hash), role, and unit assignment.
/// </summary>
public sealed class Resident : AggregateRoot
{
    public Guid ComplexId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string UnitNumber { get; private set; }
    public ResidentRole Role { get; private set; }
    public ResidentStatus Status { get; private set; }

    // Password recovery
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    // Required by EF Core
    private Resident()
    {
        FirstName = string.Empty; LastName = string.Empty;
        Email = null!; PasswordHash = string.Empty; UnitNumber = string.Empty;
    }

    private Resident(
        Guid complexId, string firstName, string lastName,
        Email email, string passwordHash,
        string unitNumber, ResidentRole role)
    {
        ComplexId    = complexId;
        FirstName    = firstName;
        LastName     = lastName;
        Email        = email;
        PasswordHash = passwordHash;
        UnitNumber   = unitNumber;
        Role         = role;
        Status       = ResidentStatus.Active;
    }

    /// <summary>
    /// Called when a resident completes registration from an invite.
    /// Password hashing must happen in the Application layer before calling this.
    /// </summary>
    public static Resident Register(
        Guid complexId,
        string firstName,
        string lastName,
        Email email,
        string passwordHash,
        string unitNumber,
        ResidentRole role = ResidentRole.Owner)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(unitNumber))
            throw new DomainException("Unit number is required.");

        var resident = new Resident(
            complexId, firstName.Trim(), lastName.Trim(),
            email, passwordHash, unitNumber.Trim().ToUpperInvariant(), role);

        resident.RaiseDomainEvent(new ResidentRegisteredEvent(resident.Id, complexId, email.Value));
        return resident;
    }

    // ── Password recovery ──────────────────────────────────────────────────

    public string GeneratePasswordResetToken()
    {
        if (Status != ResidentStatus.Active)
            throw new DomainException("Only active residents can request a password reset.");

        PasswordResetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                    .Replace("=", "").Replace("+", "-").Replace("/", "_");

        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        MarkUpdated();
        return PasswordResetToken;
    }

    public void ResetPassword(string token, string newPasswordHash)
    {
        if (PasswordResetToken is null || PasswordResetTokenExpiresAt is null)
            throw new DomainException("No password reset has been requested.");

        if (PasswordResetToken != token)
            throw new DomainException("Invalid password reset token.");

        if (DateTime.UtcNow > PasswordResetTokenExpiresAt)
            throw new DomainException("Password reset token has expired.");

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        MarkUpdated();

        RaiseDomainEvent(new ResidentPasswordResetEvent(Id, ComplexId));
    }

    // ── Profile ────────────────────────────────────────────────────────────

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))  throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName  = lastName.Trim();
        MarkUpdated();
    }

    public void ChangeUnit(string newUnitNumber)
    {
        if (string.IsNullOrWhiteSpace(newUnitNumber))
            throw new DomainException("Unit number is required.");

        UnitNumber = newUnitNumber.Trim().ToUpperInvariant();
        MarkUpdated();
    }

    // ── Status management (called by Admin/Trustee) ────────────────────────

    public void Suspend()
    {
        if (Status != ResidentStatus.Active)
            throw new DomainException("Only active residents can be suspended.");

        Status = ResidentStatus.Suspended;
        MarkUpdated();
    }

    public void Reinstate()
    {
        if (Status != ResidentStatus.Suspended)
            throw new DomainException("Only suspended residents can be reinstated.");

        Status = ResidentStatus.Active;
        MarkUpdated();
    }

    public void Remove()
    {
        Status = ResidentStatus.Removed;
        MarkUpdated();
    }

    public void PromoteToTrustee()
    {
        if (Status != ResidentStatus.Active)
            throw new DomainException("Only active residents can be promoted.");

        Role = ResidentRole.Trustee;
        MarkUpdated();
    }
}
