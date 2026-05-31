using UnitConnect.Domain.Aggregates.Complexes;
using UnitConnect.Domain.Aggregates.Listings;
using UnitConnect.Domain.Aggregates.Notices;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Enums;

namespace UnitConnect.Domain.Repositories;

public interface IComplexRepository
{
    Task<Complex?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Complex complex, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public interface IResidentRepository
{
    Task<Resident?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Resident?> GetByEmailAsync(string email, Guid complexId, CancellationToken ct = default);
    Task<Resident?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<Resident>> GetByComplexAsync(Guid complexId, CancellationToken ct = default);
    Task<bool> EmailExistsInComplexAsync(string email, Guid complexId, CancellationToken ct = default);
    Task AddAsync(Resident resident, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public interface IResidentInviteRepository
{
    Task<ResidentInvite?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<ResidentInvite?> GetPendingByEmailAndComplexAsync(string email, Guid complexId, CancellationToken ct = default);
    Task AddAsync(ResidentInvite invite, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Listing>> GetActiveByComplexAsync(
        Guid complexId,
        ListingCategory? category = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<Listing>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
    Task AddAsync(Listing listing, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public interface INoticeRepository
{
    Task<Notice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Notice>> GetPublishedByComplexAsync(Guid complexId, CancellationToken ct = default);
    Task<IReadOnlyList<Notice>> GetUrgentUnacknowledgedAsync(Guid complexId, Guid residentId, CancellationToken ct = default);
    Task AddAsync(Notice notice, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
