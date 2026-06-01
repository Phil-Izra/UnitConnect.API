using UnitConnect.Application.Abstractions;
using UnitConnect.Application.Exceptions;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Repositories;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Application.Features.Residents;

public sealed class ResidentService(
    IResidentRepository residentRepository,
    IResidentInviteRepository inviteRepository,
    IEmailService emailService,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IResidentService
{
    public async Task<Guid> InviteAsync(
        Guid complexId, InviteResidentRequest request,
        Guid invitedById, CancellationToken ct = default)
    {
        var email = Email.Create(request.Email);

        if (await residentRepository.EmailExistsInComplexAsync(email.Value, complexId, ct))
            throw new ConflictException($"A resident with email {email.Value} already exists in this complex.");

        if (await inviteRepository.GetPendingByEmailAndComplexAsync(email.Value, complexId, ct) is not null)
            throw new ConflictException("A pending invite already exists for this email.");

        var invite = ResidentInvite.Create(complexId, email, request.UnitNumber, request.IntendedRole, invitedById);
        await inviteRepository.AddAsync(invite, ct);
        await inviteRepository.SaveAsync(ct);

        await emailService.SendResidentInviteEmailAsync(email.Value, "your complex", invite.Token, ct);
        return invite.Id;
    }

    public async Task<Guid> RegisterAsync(RegisterResidentRequest request, CancellationToken ct = default)
    {
        var invite = await inviteRepository.GetByTokenAsync(request.InviteToken, ct)
            ?? throw new NotFoundException("Invite not found or already used.");

        // Accept() throws DomainException if expired or already consumed
        invite.Accept();

        if (await residentRepository.EmailExistsInComplexAsync(invite.InvitedEmail.Value, invite.ComplexId, ct))
            throw new ConflictException("A resident with this email already exists.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var resident = Resident.Register(
            invite.ComplexId,
            request.FirstName,
            request.LastName,
            invite.InvitedEmail,
            passwordHash,
            invite.UnitNumber,
            invite.IntendedRole);

        await residentRepository.AddAsync(resident, ct);
        // Both changes live in the same DbContext, so one SaveAsync commits everything atomically
        await residentRepository.SaveAsync(ct);
        return resident.Id;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByEmailAsync(request.Email, request.ComplexId, ct)
            ?? throw new NotFoundException("Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, resident.PasswordHash))
            throw new NotFoundException("Invalid email or password."); // same message to prevent email enumeration

        return new LoginResponse(jwtService.GenerateToken(resident), ToResponse(resident));
    }

    public async Task RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByEmailAsync(request.Email, request.ComplexId, ct);
        if (resident is null) return; // silently ignore — prevents email enumeration

        var token = resident.GeneratePasswordResetToken();
        await residentRepository.SaveAsync(ct);
        await emailService.SendPasswordResetEmailAsync(resident.Email.Value, token, ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByPasswordResetTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("Invalid or expired reset token.");

        resident.ResetPassword(request.Token, passwordHasher.Hash(request.NewPassword));
        await residentRepository.SaveAsync(ct);
    }

    public async Task UpdateProfileAsync(Guid residentId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct)
            ?? throw new NotFoundException($"Resident {residentId} not found.");

        resident.UpdateProfile(request.FirstName, request.LastName);
        await residentRepository.SaveAsync(ct);
    }

    public async Task SuspendAsync(Guid residentId, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct)
            ?? throw new NotFoundException($"Resident {residentId} not found.");

        resident.Suspend();
        await residentRepository.SaveAsync(ct);
    }

    public async Task ReinstateAsync(Guid residentId, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct)
            ?? throw new NotFoundException($"Resident {residentId} not found.");

        resident.Reinstate();
        await residentRepository.SaveAsync(ct);
    }

    public async Task RemoveAsync(Guid residentId, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct)
            ?? throw new NotFoundException($"Resident {residentId} not found.");

        resident.Remove();
        await residentRepository.SaveAsync(ct);
    }

    public async Task PromoteToTrusteeAsync(Guid residentId, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct)
            ?? throw new NotFoundException($"Resident {residentId} not found.");

        resident.PromoteToTrustee();
        await residentRepository.SaveAsync(ct);
    }

    public async Task<IReadOnlyList<ResidentResponse>> GetByComplexAsync(Guid complexId, CancellationToken ct = default)
    {
        var residents = await residentRepository.GetByComplexAsync(complexId, ct);
        return residents.Select(ToResponse).ToList();
    }

    public async Task<ResidentResponse?> GetByIdAsync(Guid residentId, CancellationToken ct = default)
    {
        var resident = await residentRepository.GetByIdAsync(residentId, ct);
        return resident is null ? null : ToResponse(resident);
    }

    private static ResidentResponse ToResponse(Resident r) => new(
        r.Id, r.ComplexId, r.FirstName, r.LastName,
        r.Email.Value, r.UnitNumber, r.Role, r.Status);
}
