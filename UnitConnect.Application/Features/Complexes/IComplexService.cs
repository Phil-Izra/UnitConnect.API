namespace UnitConnect.Application.Features.Complexes;

public interface IComplexService
{
    Task<Guid> CreateAsync(CreateComplexRequest request, CancellationToken ct = default);
    Task UpdateDetailsAsync(Guid complexId, UpdateComplexRequest request, CancellationToken ct = default);
    Task<ComplexUnitResponse> AddUnitAsync(Guid complexId, string unitNumber, CancellationToken ct = default);
    Task<ComplexResponse?> GetByIdAsync(Guid complexId, CancellationToken ct = default);
}
