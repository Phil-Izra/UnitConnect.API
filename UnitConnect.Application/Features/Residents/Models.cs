using UnitConnect.Domain.Enums;

namespace UnitConnect.Application.Features.Residents;

public record InviteResidentRequest(
    string Email,
    string UnitNumber,
    ResidentRole IntendedRole);

public record RegisterResidentRequest(
    string InviteToken,
    string FirstName,
    string LastName,
    string Password);

public record LoginRequest(
    Guid ComplexId,
    string Email,
    string Password);

public record UpdateProfileRequest(
    string FirstName,
    string LastName);

public record RequestPasswordResetRequest(
    Guid ComplexId,
    string Email);

public record ResetPasswordRequest(
    string Token,
    string NewPassword);

public record ResidentResponse(
    Guid Id,
    Guid ComplexId,
    string FirstName,
    string LastName,
    string Email,
    string UnitNumber,
    ResidentRole Role,
    ResidentStatus Status);

public record LoginResponse(
    string Token,
    ResidentResponse Resident);
