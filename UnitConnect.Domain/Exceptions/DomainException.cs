namespace UnitConnect.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated within the domain.
/// Application layer catches this and maps it to an appropriate HTTP response.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
