using FluentAssertions;
using UnitConnect.Domain.Aggregates.Listings;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Domain;

public sealed class ListingTests
{
    private static readonly Guid ComplexId = Guid.NewGuid();
    private static readonly Guid SellerId  = Guid.NewGuid();

    private static Listing MakeListing(string title = "Old Couch") =>
        Listing.Create(ComplexId, SellerId, title, "Good condition", Money.Create(500, "ZAR"), ListingCategory.Furniture);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsStatusActiveAndRaisesEvent()
    {
        var listing = MakeListing();

        listing.Status.Should().Be(ListingStatus.Active);
        listing.ComplexId.Should().Be(ComplexId);
        listing.SellerId.Should().Be(SellerId);
        listing.DomainEvents.OfType<ListingCreatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Create_TitleExceeds120Chars_ThrowsDomainException()
    {
        var longTitle = new string('A', 121);
        var act = () => MakeListing(longTitle);
        act.Should().Throw<DomainException>().WithMessage("*120*");
    }

    // ── UpdateDetails ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_ActiveListing_UpdatesFields()
    {
        var listing = MakeListing();
        listing.UpdateDetails("New Title", "New Desc", Money.Create(300, "ZAR"), ListingCategory.Electronics);

        listing.Title.Should().Be("New Title");
        listing.Price.Amount.Should().Be(300);
    }

    [Fact]
    public void UpdateDetails_SoldListing_ThrowsDomainException()
    {
        var listing = MakeListing();
        listing.MarkAsSold();

        var act = () => listing.UpdateDetails("New", "Desc", Money.Create(100, "ZAR"), ListingCategory.Other);
        act.Should().Throw<DomainException>();
    }

    // ── Images ────────────────────────────────────────────────────────────────

    [Fact]
    public void AddImage_UnderLimit_AddsImage()
    {
        var listing = MakeListing();
        listing.AddImage("storage/img1.jpg");
        listing.Images.Should().ContainSingle();
    }

    [Fact]
    public void AddImage_AtSixImages_ThrowsDomainException()
    {
        var listing = MakeListing();
        for (var i = 0; i < 6; i++) listing.AddImage($"img{i}.jpg");

        var act = () => listing.AddImage("img7.jpg");
        act.Should().Throw<DomainException>().WithMessage("*6*");
    }

    [Fact]
    public void RemoveImage_ExistingImage_RemovesIt()
    {
        var listing = MakeListing();
        listing.AddImage("img1.jpg");
        var imageId = listing.Images.First().Id;

        listing.RemoveImage(imageId);

        listing.Images.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_NonExistentId_ThrowsDomainException()
    {
        var listing = MakeListing();
        var act = () => listing.RemoveImage(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    // ── Status transitions ────────────────────────────────────────────────────

    [Fact]
    public void MarkAsSold_ActiveListing_SetsSoldAndRaisesEvent()
    {
        var listing = MakeListing();
        listing.MarkAsSold();

        listing.Status.Should().Be(ListingStatus.Sold);
        listing.DomainEvents.OfType<ListingSoldEvent>().Should().ContainSingle();
    }

    [Fact]
    public void MarkAsSold_AlreadySold_ThrowsDomainException()
    {
        var listing = MakeListing();
        listing.MarkAsSold();

        var act = () => listing.MarkAsSold();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Withdraw_ActiveListing_SetsWithdrawnStatus()
    {
        var listing = MakeListing();
        listing.Withdraw();
        listing.Status.Should().Be(ListingStatus.Withdrawn);
    }

    // ── Contact Request ───────────────────────────────────────────────────────

    [Fact]
    public void RequestContact_DifferentBuyer_CreatesContactRequestAndRaisesEvent()
    {
        var listing = MakeListing();
        var buyerId = Guid.NewGuid();

        var cr = listing.RequestContact(buyerId);

        cr.BuyerId.Should().Be(buyerId);
        listing.DomainEvents.OfType<ListingContactRequestedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RequestContact_SellerContactsSelf_ThrowsDomainException()
    {
        var listing = MakeListing();
        var act = () => listing.RequestContact(SellerId);
        act.Should().Throw<DomainException>().WithMessage("*yourself*");
    }

    [Fact]
    public void RequestContact_DuplicateBuyer_ThrowsDomainException()
    {
        var listing = MakeListing();
        var buyerId = Guid.NewGuid();
        listing.RequestContact(buyerId);

        var act = () => listing.RequestContact(buyerId);
        act.Should().Throw<DomainException>();
    }
}
