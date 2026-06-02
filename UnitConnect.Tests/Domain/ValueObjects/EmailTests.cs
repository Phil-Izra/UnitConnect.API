using FluentAssertions;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("User@Example.COM")]
    [InlineData("first.last+tag@sub.domain.co.za")]
    public void Create_ValidEmail_NormalisesToLowercase(string raw)
    {
        var email = Email.Create(raw);
        email.Value.Should().Be(raw.ToLowerInvariant().Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankEmail_ThrowsDomainException(string raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("missing@")]
    [InlineData("spaces in @email.com")]
    public void Create_InvalidFormat_ThrowsDomainException(string raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TwoEmailsWithSameValue_AreEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");
        a.Should().Be(b);
    }
}
