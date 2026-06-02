using FluentAssertions;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Domain;

public sealed class ResidentTests
{
    private static Resident MakeResident(ResidentStatus? status = null)
    {
        var resident = Resident.Register(
            Guid.NewGuid(), "Jane", "Doe",
            Email.Create("jane@example.com"), "hash123", "A1");

        // Manipulate status via domain methods where needed
        if (status == ResidentStatus.Suspended) resident.Suspend();
        return resident;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public void Register_WithValidData_SetsPropertiesAndRaisesEvent()
    {
        var email    = Email.Create("jane@example.com");
        var resident = Resident.Register(Guid.NewGuid(), "Jane", "Doe", email, "hash", "A1");

        resident.FirstName.Should().Be("Jane");
        resident.LastName.Should().Be("Doe");
        resident.Email.Value.Should().Be("jane@example.com");
        resident.UnitNumber.Should().Be("A1");
        resident.Status.Should().Be(ResidentStatus.Active);
        resident.Role.Should().Be(ResidentRole.Owner);
        resident.DomainEvents.OfType<ResidentRegisteredEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithBlankFirstName_ThrowsDomainException(string name)
    {
        var act = () => Resident.Register(Guid.NewGuid(), name, "Doe", Email.Create("a@b.com"), "hash", "A1");
        act.Should().Throw<DomainException>();
    }

    // ── Password Reset ────────────────────────────────────────────────────────

    [Fact]
    public void GeneratePasswordResetToken_ActiveResident_ReturnsTokenAndSetsExpiry()
    {
        var resident = MakeResident();

        var token = resident.GeneratePasswordResetToken();

        token.Should().NotBeNullOrEmpty();
        resident.PasswordResetToken.Should().Be(token);
        resident.PasswordResetTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GeneratePasswordResetToken_SuspendedResident_ThrowsDomainException()
    {
        var resident = MakeResident(ResidentStatus.Suspended);

        var act = () => resident.GeneratePasswordResetToken();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ResetPassword_WithValidToken_UpdatesHashAndClearsToken()
    {
        var resident = MakeResident();
        var token    = resident.GeneratePasswordResetToken();

        resident.ResetPassword(token, "newHash");

        resident.PasswordHash.Should().Be("newHash");
        resident.PasswordResetToken.Should().BeNull();
        resident.DomainEvents.OfType<ResidentPasswordResetEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ResetPassword_WithWrongToken_ThrowsDomainException()
    {
        var resident = MakeResident();
        resident.GeneratePasswordResetToken();

        var act = () => resident.ResetPassword("wrong-token", "newHash");
        act.Should().Throw<DomainException>();
    }

    // ── Status Management ─────────────────────────────────────────────────────

    [Fact]
    public void Suspend_ActiveResident_SetsSuspendedStatus()
    {
        var resident = MakeResident();
        resident.Suspend();
        resident.Status.Should().Be(ResidentStatus.Suspended);
    }

    [Fact]
    public void Suspend_AlreadySuspended_ThrowsDomainException()
    {
        var resident = MakeResident();
        resident.Suspend();

        var act = () => resident.Suspend();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reinstate_SuspendedResident_SetsActiveStatus()
    {
        var resident = MakeResident();
        resident.Suspend();
        resident.Reinstate();
        resident.Status.Should().Be(ResidentStatus.Active);
    }

    [Fact]
    public void PromoteToTrustee_ActiveResident_SetsRole()
    {
        var resident = MakeResident();
        resident.PromoteToTrustee();
        resident.Role.Should().Be(ResidentRole.Trustee);
    }

    [Fact]
    public void PromoteToTrustee_SuspendedResident_ThrowsDomainException()
    {
        var resident = MakeResident();
        resident.Suspend();

        var act = () => resident.PromoteToTrustee();
        act.Should().Throw<DomainException>();
    }
}
