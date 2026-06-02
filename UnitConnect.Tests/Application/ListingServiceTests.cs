using FluentAssertions;
using Moq;
using UnitConnect.Application.Exceptions;
using UnitConnect.Application.Features.Listings;
using UnitConnect.Domain.Aggregates.Listings;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Application;

public sealed class ListingServiceTests
{
    private readonly Mock<IListingRepository> _repo = new();
    private readonly ListingService _service;

    private static readonly Guid ComplexId = Guid.NewGuid();
    private static readonly Guid SellerId  = Guid.NewGuid();

    public ListingServiceTests() => _service = new ListingService(_repo.Object);

    private static Listing MakeListing() =>
        Listing.Create(ComplexId, SellerId, "Old Couch", "Good condition", Money.Create(500, "ZAR"), ListingCategory.Furniture);

    private void SetupSave() =>
        _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsNewId()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Listing>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        SetupSave();

        var id = await _service.CreateAsync(ComplexId, SellerId,
            new CreateListingRequest("Old Couch", "Good condition", 500, "ZAR", ListingCategory.Furniture));

        id.Should().NotBe(Guid.Empty);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingListing_ReturnsResponse()
    {
        var listing = MakeListing();
        _repo.Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(listing);

        var result = await _service.GetByIdAsync(listing.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Old Couch");
        result.Status.Should().Be(ListingStatus.Active);
    }

    // ── MarkAsSoldAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsSoldAsync_ActiveListing_Saves()
    {
        var listing = MakeListing();
        _repo.Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(listing);
        SetupSave();

        await _service.MarkAsSoldAsync(listing.Id);

        _repo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task MarkAsSoldAsync_NonExistentListing_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Listing?)null);

        var act = () => _service.MarkAsSoldAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── RequestContactAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task RequestContactAsync_DifferentBuyer_ReturnsContactRequest()
    {
        var listing = MakeListing();
        var buyerId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(listing);
        SetupSave();

        var cr = await _service.RequestContactAsync(listing.Id, buyerId);

        cr.BuyerId.Should().Be(buyerId);
    }
}
