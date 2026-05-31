using UnitConnect.Domain.Common;
using UnitConnect.Domain.Exceptions;
using UnitConnect.Domain.Events;

namespace UnitConnect.Domain.Aggregates.Complexes;

/// <summary>
/// Represents a sectional title complex (the tenant boundary).
/// Every resident, listing and notice belongs to exactly one Complex.
/// </summary>
public sealed class Complex : AggregateRoot
{
    public string Name { get; private set; }
    public string PhysicalAddress { get; private set; }
    public string? AdminContactEmail { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core navigation — not a public collection to protect the invariant
    private readonly List<ComplexUnit> _units = [];
    public IReadOnlyCollection<ComplexUnit> Units => _units.AsReadOnly();

    // Required by EF Core
    private Complex() { Name = string.Empty; PhysicalAddress = string.Empty; }

    private Complex(string name, string physicalAddress, string? adminContactEmail)
    {
        Name = name;
        PhysicalAddress = physicalAddress;
        AdminContactEmail = adminContactEmail;
        IsActive = true;
    }

    public static Complex Create(string name, string physicalAddress, string? adminContactEmail = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Complex name is required.");

        if (string.IsNullOrWhiteSpace(physicalAddress))
            throw new DomainException("Complex physical address is required.");

        var complex = new Complex(name.Trim(), physicalAddress.Trim(), adminContactEmail?.Trim());
        complex.RaiseDomainEvent(new ComplexCreatedEvent(complex.Id, complex.Name));
        return complex;
    }

    public void UpdateDetails(string name, string physicalAddress, string? adminContactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Complex name is required.");

        Name = name.Trim();
        PhysicalAddress = physicalAddress.Trim();
        AdminContactEmail = adminContactEmail?.Trim();
        MarkUpdated();
    }

    public ComplexUnit AddUnit(string unitNumber)
    {
        if (string.IsNullOrWhiteSpace(unitNumber))
            throw new DomainException("Unit number is required.");

        var normalized = unitNumber.Trim().ToUpperInvariant();

        if (_units.Any(u => u.Number.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Unit '{normalized}' already exists in this complex.");

        var unit = ComplexUnit.Create(Id, normalized);
        _units.Add(unit);
        MarkUpdated();
        return unit;
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
