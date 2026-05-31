using UnitConnect.Domain.Common;

namespace UnitConnect.Domain.Aggregates.Complexes;

/// <summary>
/// A physical unit (flat/townhouse) inside a Complex.
/// Lives inside the Complex aggregate boundary.
/// </summary>
public sealed class ComplexUnit : Entity
{
    public Guid ComplexId { get; private set; }
    public string Number { get; private set; }   // e.g. "12", "B4"
    public bool IsOccupied { get; private set; }

    private ComplexUnit() { Number = string.Empty; }

    private ComplexUnit(Guid complexId, string number)
    {
        ComplexId = complexId;
        Number = number;
        IsOccupied = false;
    }

    internal static ComplexUnit Create(Guid complexId, string number) =>
        new(complexId, number);

    internal void MarkOccupied() => IsOccupied = true;
    internal void MarkVacant()   => IsOccupied = false;
}
