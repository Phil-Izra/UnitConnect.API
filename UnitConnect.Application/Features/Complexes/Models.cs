namespace UnitConnect.Application.Features.Complexes;

public record CreateComplexRequest(
    string Name,
    string PhysicalAddress,
    string? AdminContactEmail);

public record UpdateComplexRequest(
    string Name,
    string PhysicalAddress,
    string? AdminContactEmail);

public record ComplexUnitResponse(
    Guid Id,
    string Number,
    bool IsOccupied);

public record ComplexResponse(
    Guid Id,
    string Name,
    string PhysicalAddress,
    string? AdminContactEmail,
    bool IsActive,
    IReadOnlyList<ComplexUnitResponse> Units);
