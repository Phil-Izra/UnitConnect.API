using FluentAssertions;
using Moq;
using UnitConnect.Application.Exceptions;
using UnitConnect.Application.Features.Complexes;
using UnitConnect.Domain.Aggregates.Complexes;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Tests.Application;

public sealed class ComplexServiceTests
{
    private readonly Mock<IComplexRepository> _repo = new();
    private readonly ComplexService _service;

    public ComplexServiceTests() => _service = new ComplexService(_repo.Object);

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsNewId()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Complex>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var id = await _service.CreateAsync(new CreateComplexRequest("Sunset Villas", "10 Main Rd", null));

        id.Should().NotBe(Guid.Empty);
        _repo.Verify(r => r.AddAsync(It.IsAny<Complex>(), default), Times.Once);
        _repo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingComplex_ReturnsResponse()
    {
        var complex = Complex.Create("Sunset Villas", "10 Main Rd");
        _repo.Setup(r => r.GetByIdAsync(complex.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(complex);

        var result = await _service.GetByIdAsync(complex.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Sunset Villas");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentComplex_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Complex?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── UpdateDetailsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateDetailsAsync_ExistingComplex_Saves()
    {
        var complex = Complex.Create("Old", "Old Address");
        _repo.Setup(r => r.GetByIdAsync(complex.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(complex);
        _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await _service.UpdateDetailsAsync(complex.Id, new UpdateComplexRequest("New", "New Address", null));

        _repo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateDetailsAsync_NonExistentComplex_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Complex?)null);

        var act = () => _service.UpdateDetailsAsync(Guid.NewGuid(), new UpdateComplexRequest("N", "A", null));
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── AddUnitAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AddUnitAsync_ExistingComplex_ReturnsUnitResponse()
    {
        var complex = Complex.Create("Test", "Address");
        _repo.Setup(r => r.GetByIdAsync(complex.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(complex);
        _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var unit = await _service.AddUnitAsync(complex.Id, "B4");

        unit.Number.Should().Be("B4");
        unit.IsOccupied.Should().BeFalse();
    }
}
