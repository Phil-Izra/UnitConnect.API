using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitConnect.API.Extensions;
using UnitConnect.Application.Features.Residents;

namespace UnitConnect.API.Controllers;

[ApiController]
[Route("api")]
public sealed class ResidentsController(IResidentService residentService) : ControllerBase
{
    // ── Auth ──────────────────────────────────────────────────────────────────

    [HttpPost("auth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await residentService.LoginAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("auth/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterResidentRequest request, CancellationToken ct)
    {
        var id = await residentService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("auth/password-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request, CancellationToken ct)
    {
        await residentService.RequestPasswordResetAsync(request, ct);
        return Ok(new { message = "If that email exists, a reset link has been sent." });
    }

    [HttpPost("auth/password-reset/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await residentService.ResetPasswordAsync(request, ct);
        return NoContent();
    }

    // ── Invites ───────────────────────────────────────────────────────────────

    [HttpPost("complexes/{complexId:guid}/residents/invite")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Invite(Guid complexId, [FromBody] InviteResidentRequest request, CancellationToken ct)
    {
        var invitedById = User.GetResidentId();
        var id = await residentService.InviteAsync(complexId, request, invitedById, ct);
        return StatusCode(StatusCodes.Status201Created, new { id });
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    [HttpGet("complexes/{complexId:guid}/residents")]
    [Authorize]
    public async Task<IActionResult> GetByComplex(Guid complexId, CancellationToken ct)
    {
        var residents = await residentService.GetByComplexAsync(complexId, ct);
        return Ok(residents);
    }

    [HttpGet("residents/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var resident = await residentService.GetByIdAsync(id, ct);
        return resident is null ? NotFound() : Ok(resident);
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    [HttpPut("residents/{id:guid}/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        await residentService.UpdateProfileAsync(id, request, ct);
        return NoContent();
    }

    // ── Admin / Trustee actions ───────────────────────────────────────────────

    [HttpPost("residents/{id:guid}/suspend")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        await residentService.SuspendAsync(id, ct);
        return NoContent();
    }

    [HttpPost("residents/{id:guid}/reinstate")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Reinstate(Guid id, CancellationToken ct)
    {
        await residentService.ReinstateAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("residents/{id:guid}")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        await residentService.RemoveAsync(id, ct);
        return NoContent();
    }

    [HttpPost("residents/{id:guid}/promote")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> PromoteToTrustee(Guid id, CancellationToken ct)
    {
        await residentService.PromoteToTrusteeAsync(id, ct);
        return NoContent();
    }
}
