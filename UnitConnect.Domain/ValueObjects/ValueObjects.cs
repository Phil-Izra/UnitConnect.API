using System.Text.RegularExpressions;
using UnitConnect.Domain.Exceptions;

namespace UnitConnect.Domain.ValueObjects;

/// <summary>
/// Immutable, self-validating email address.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email address cannot be empty.");

        var trimmed = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(trimmed);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Unit number within a complex (e.g. "12", "B4", "P1-05").
/// </summary>
public sealed record UnitNumber
{
    public string Value { get; }

    private UnitNumber(string value) => Value = value;

    public static UnitNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Unit number cannot be empty.");

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length > 20)
            throw new DomainException("Unit number cannot exceed 20 characters.");

        return new UnitNumber(trimmed);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Monetary amount with currency. Keeps pricing simple and explicit.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217 e.g. "ZAR"

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "ZAR")
    {
        if (amount < 0)
            throw new DomainException("Price cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Free() => new(0, "ZAR");

    public override string ToString() => $"{Currency} {Amount:N2}";
}
