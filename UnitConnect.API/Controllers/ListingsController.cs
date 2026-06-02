using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitConnect.API.Extensions;
using UnitConnect.Application.Features.Listings;
using UnitConnect.Domain.Enums;

namespace UnitConnect.API.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public sealed class ListingsController(IListingService listingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListingRequest request, CancellationToken ct)
    {
        var sellerId  = User.GetResidentId();
        var complexId = User.GetComplexId();
        var id = await listingService.CreateAsync(complexId, sellerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var listing = await listingService.GetByIdAsync(id, ct);
        return listing is null ? NotFound() : Ok(listing);
    }

    [HttpGet("~/api/complexes/{complexId:guid}/listings")]
    public async Task<IActionResult> GetActiveByComplex(
        Guid complexId,
        [FromQuery] ListingCategory? category,
        CancellationToken ct)
    {
        var listings = await listingService.GetActiveByComplexAsync(complexId, category, ct);
        return Ok(listings);
    }

    [HttpGet("~/api/residents/{sellerId:guid}/listings")]
    public async Task<IActionResult> GetBySeller(Guid sellerId, CancellationToken ct)
    {
        var listings = await listingService.GetBySellerAsync(sellerId, ct);
        return Ok(listings);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingRequest request, CancellationToken ct)
    {
        await listingService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, [FromBody] AddImageRequest request, CancellationToken ct)
    {
        await listingService.AddImageAsync(id, request.StoragePath, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken ct)
    {
        await listingService.RemoveImageAsync(id, imageId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/sold")]
    public async Task<IActionResult> MarkAsSold(Guid id, CancellationToken ct)
    {
        await listingService.MarkAsSoldAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken ct)
    {
        await listingService.WithdrawAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/contact")]
    public async Task<IActionResult> RequestContact(Guid id, CancellationToken ct)
    {
        var buyerId = User.GetResidentId();
        var result  = await listingService.RequestContactAsync(id, buyerId, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}

public record AddImageRequest(string StoragePath);
