namespace UnitConnect.Application.Features.Residents;

public interface IResidentService
{
    Task<Guid> InviteAsync(Guid complexId, InviteResidentRequest request, Guid invitedById, CancellationToken ct = default);
    Task<Guid> RegisterAsync(RegisterResidentRequest request, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task UpdateProfileAsync(Guid residentId, UpdateProfileRequest request, CancellationToken ct = default);
    Task SuspendAsync(Guid residentId, CancellationToken ct = default);
    Task ReinstateAsync(Guid residentId, CancellationToken ct = default);
    Task RemoveAsync(Guid residentId, CancellationToken ct = default);
    Task PromoteToTrusteeAsync(Guid residentId, CancellationToken ct = default);
    Task<IReadOnlyList<ResidentResponse>> GetByComplexAsync(Guid complexId, CancellationToken ct = default);
    Task<ResidentResponse?> GetByIdAsync(Guid residentId, CancellationToken ct = default);
}
