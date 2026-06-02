using FluentAssertions;
using UnitConnect.Domain.Aggregates.Complexes;
using UnitConnect.Domain.Events;
using UnitConnect.Domain.Exceptions;

namespace UnitConnect.Tests.Domain;

public sealed class ComplexTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsPropertiesAndRaisesEvent()
    {
        var complex = Complex.Create("Sunset Villas", "10 Main Rd", "admin@sunset.co.za");

        complex.Name.Should().Be("Sunset Villas");
        complex.PhysicalAddress.Should().Be("10 Main Rd");
        complex.AdminContactEmail.Should().Be("admin@sunset.co.za");
        complex.IsActive.Should().BeTrue();
        complex.DomainEvents.OfType<ComplexCreatedEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ThrowsDomainException(string name)
    {
        var act = () => Complex.Create(name, "10 Main Rd");
        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankAddress_ThrowsDomainException(string address)
    {
        var act = () => Complex.Create("Sunset Villas", address);
        act.Should().Throw<DomainException>().WithMessage("*address*");
    }

    // ── AddUnit ───────────────────────────────────────────────────────────────

    [Fact]
    public void AddUnit_WithValidNumber_AddsUnitToCollection()
    {
        var complex = Complex.Create("Test", "Address");

        var unit = complex.AddUnit("A1");

        complex.Units.Should().ContainSingle();
        unit.Number.Should().Be("A1");
        unit.IsOccupied.Should().BeFalse();
    }

    [Fact]
    public void AddUnit_NormalisesToUpperCase()
    {
        var complex = Complex.Create("Test", "Address");

        var unit = complex.AddUnit("b2");

        unit.Number.Should().Be("B2");
    }

    [Fact]
    public void AddUnit_WithDuplicateNumber_ThrowsDomainException()
    {
        var complex = Complex.Create("Test", "Address");
        complex.AddUnit("A1");

        var act = () => complex.AddUnit("a1"); // case-insensitive duplicate
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddUnit_WithBlankNumber_ThrowsDomainException()
    {
        var complex = Complex.Create("Test", "Address");

        var act = () => complex.AddUnit("   ");
        act.Should().Throw<DomainException>();
    }

    // ── UpdateDetails ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesProperties()
    {
        var complex = Complex.Create("Old Name", "Old Address");

        complex.UpdateDetails("New Name", "New Address", "new@email.com");

        complex.Name.Should().Be("New Name");
        complex.PhysicalAddress.Should().Be("New Address");
        complex.UpdatedAt.Should().NotBeNull();
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var complex = Complex.Create("Test", "Address");

        complex.Deactivate();

        complex.IsActive.Should().BeFalse();
    }
}
