using UnitConnect.Application.Exceptions;
using UnitConnect.Domain.Aggregates.Complexes;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Application.Features.Complexes;

public sealed class ComplexService(IComplexRepository complexRepository) : IComplexService
{
    public async Task<Guid> CreateAsync(CreateComplexRequest request, CancellationToken ct = default)
    {
        var complex = Complex.Create(request.Name, request.PhysicalAddress, request.AdminContactEmail);
        await complexRepository.AddAsync(complex, ct);
        await complexRepository.SaveAsync(ct);
        return complex.Id;
    }

    public async Task UpdateDetailsAsync(Guid complexId, UpdateComplexRequest request, CancellationToken ct = default)
    {
        var complex = await complexRepository.GetByIdAsync(complexId, ct)
            ?? throw new NotFoundException($"Complex {complexId} not found.");

        complex.UpdateDetails(request.Name, request.PhysicalAddress, request.AdminContactEmail);
        await complexRepository.SaveAsync(ct);
    }

    public async Task<ComplexUnitResponse> AddUnitAsync(Guid complexId, string unitNumber, CancellationToken ct = default)
    {
        var complex = await complexRepository.GetByIdAsync(complexId, ct)
            ?? throw new NotFoundException($"Complex {complexId} not found.");

        var unit = complex.AddUnit(unitNumber);
        await complexRepository.SaveAsync(ct);
        return new ComplexUnitResponse(unit.Id, unit.Number, unit.IsOccupied);
    }

    public async Task<ComplexResponse?> GetByIdAsync(Guid complexId, CancellationToken ct = default)
    {
        var complex = await complexRepository.GetByIdAsync(complexId, ct);
        return complex is null ? null : ToResponse(complex);
    }

    private static ComplexResponse ToResponse(Complex c) => new(
        c.Id,
        c.Name,
        c.PhysicalAddress,
        c.AdminContactEmail,
        c.IsActive,
        c.Units.Select(u => new ComplexUnitResponse(u.Id, u.Number, u.IsOccupied)).ToList());
}
