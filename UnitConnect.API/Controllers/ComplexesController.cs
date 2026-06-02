using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitConnect.Application.Features.Complexes;

namespace UnitConnect.API.Controllers;

[ApiController]
[Route("api/complexes")]
[Authorize]
public sealed class ComplexesController(IComplexService complexService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateComplexRequest request, CancellationToken ct)
    {
        var id = await complexService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var complex = await complexService.GetByIdAsync(id, ct);
        return complex is null ? NotFound() : Ok(complex);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> UpdateDetails(Guid id, [FromBody] UpdateComplexRequest request, CancellationToken ct)
    {
        await complexService.UpdateDetailsAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/units")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> AddUnit(Guid id, [FromBody] AddUnitRequest request, CancellationToken ct)
    {
        var unit = await complexService.AddUnitAsync(id, request.UnitNumber, ct);
        return StatusCode(StatusCodes.Status201Created, unit);
    }
}

public record AddUnitRequest(string UnitNumber);
