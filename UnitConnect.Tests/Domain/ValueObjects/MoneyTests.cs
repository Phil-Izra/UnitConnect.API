using FluentAssertions;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithPositiveAmount_SetsProperties()
    {
        var money = Money.Create(1500.50m, "ZAR");

        money.Amount.Should().Be(1500.50m);
        money.Currency.Should().Be("ZAR");
    }

    [Fact]
    public void Create_WithZeroAmount_Succeeds()
    {
        var money = Money.Create(0, "ZAR");
        money.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Create(-1, "ZAR");
        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ZA")]      // too short
    [InlineData("ZARR")]    // too long
    [InlineData("   ")]
    public void Create_WithInvalidCurrencyCode_ThrowsDomainException(string currency)
    {
        var act = () => Money.Create(100, currency);
        act.Should().Throw<DomainException>().WithMessage("*ISO*");
    }

    [Fact]
    public void Create_CurrencyIsNormalisedToUpperCase()
    {
        var money = Money.Create(100, "zar");
        money.Currency.Should().Be("ZAR");
    }

    [Fact]
    public void Free_ReturnsZeroZAR()
    {
        var money = Money.Free();
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("ZAR");
    }

    [Fact]
    public void TwoMoneyWithSameValues_AreEqual()
    {
        var a = Money.Create(500, "ZAR");
        var b = Money.Create(500, "ZAR");
        a.Should().Be(b);
    }
}
