using FluentAssertions;
using UnitConnect.Domain.Aggregates.Notices;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;

namespace UnitConnect.Tests.Domain;

public sealed class NoticeTests
{
    private static readonly Guid ComplexId  = Guid.NewGuid();
    private static readonly Guid PostedById = Guid.NewGuid();

    private static Notice MakeNotice(NoticeUrgency urgency = NoticeUrgency.General) =>
        Notice.Create(ComplexId, PostedById, "Water Outage", "Water will be off 10am–2pm.", urgency);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsPublishedAndRaisesEvent()
    {
        var notice = MakeNotice();

        notice.Status.Should().Be(NoticeStatus.Published);
        notice.Title.Should().Be("Water Outage");
        notice.DomainEvents.OfType<NoticePublishedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Create_TitleExceeds200Chars_ThrowsDomainException()
    {
        var longTitle = new string('X', 201);
        var act = () => Notice.Create(ComplexId, PostedById, longTitle, "Body");
        act.Should().Throw<DomainException>().WithMessage("*200*");
    }

    [Fact]
    public void Create_ExpiryInPast_ThrowsDomainException()
    {
        var act = () => Notice.Create(ComplexId, PostedById, "T", "B", expiresAt: DateTime.UtcNow.AddHours(-1));
        act.Should().Throw<DomainException>().WithMessage("*future*");
    }

    // ── RequiresPushNotification ──────────────────────────────────────────────

    [Fact]
    public void RequiresPushNotification_UrgentNotice_ReturnsTrue()
    {
        var notice = MakeNotice(NoticeUrgency.Urgent);
        notice.RequiresPushNotification().Should().BeTrue();
    }

    [Fact]
    public void RequiresPushNotification_GeneralNotice_ReturnsFalse()
    {
        var notice = MakeNotice(NoticeUrgency.General);
        notice.RequiresPushNotification().Should().BeFalse();
    }

    // ── EditContent ───────────────────────────────────────────────────────────

    [Fact]
    public void EditContent_PublishedNotice_UpdatesTitleAndBody()
    {
        var notice = MakeNotice();
        notice.EditContent("Updated Title", "Updated body.");

        notice.Title.Should().Be("Updated Title");
        notice.Body.Should().Be("Updated body.");
    }

    [Fact]
    public void EditContent_ArchivedNotice_ThrowsDomainException()
    {
        var notice = MakeNotice();
        notice.Archive();

        var act = () => notice.EditContent("New", "Body");
        act.Should().Throw<DomainException>();
    }

    // ── Acknowledge ───────────────────────────────────────────────────────────

    [Fact]
    public void Acknowledge_NewResident_AddsAcknowledgement()
    {
        var notice     = MakeNotice();
        var residentId = Guid.NewGuid();

        notice.Acknowledge(residentId);

        notice.Acknowledgements.Should().ContainSingle(a => a.ResidentId == residentId);
    }

    [Fact]
    public void Acknowledge_SameResidentTwice_IsIdempotent()
    {
        var notice     = MakeNotice();
        var residentId = Guid.NewGuid();
        notice.Acknowledge(residentId);
        notice.Acknowledge(residentId); // second call must not throw or duplicate

        notice.Acknowledgements.Should().ContainSingle();
    }

    // ── Archive ───────────────────────────────────────────────────────────────

    [Fact]
    public void Archive_PublishedNotice_SetsArchivedStatus()
    {
        var notice = MakeNotice();
        notice.Archive();
        notice.Status.Should().Be(NoticeStatus.Archived);
    }

    [Fact]
    public void Archive_AlreadyArchived_ThrowsDomainException()
    {
        var notice = MakeNotice();
        notice.Archive();

        var act = () => notice.Archive();
        act.Should().Throw<DomainException>();
    }
}
