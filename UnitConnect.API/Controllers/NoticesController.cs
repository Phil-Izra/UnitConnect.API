using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitConnect.API.Extensions;
using UnitConnect.Application.Features.Notices;

namespace UnitConnect.API.Controllers;

[ApiController]
[Route("api/notices")]
[Authorize]
public sealed class NoticesController(INoticeService noticeService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Publish([FromBody] PublishNoticeRequest request, CancellationToken ct)
    {
        var complexId          = User.GetComplexId();
        var postedByResidentId = User.GetResidentId();
        var id = await noticeService.PublishAsync(complexId, postedByResidentId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var notice = await noticeService.GetByIdAsync(id, ct);
        return notice is null ? NotFound() : Ok(notice);
    }

    [HttpGet("~/api/complexes/{complexId:guid}/notices")]
    public async Task<IActionResult> GetByComplex(Guid complexId, CancellationToken ct)
    {
        var notices = await noticeService.GetPublishedByComplexAsync(complexId, ct);
        return Ok(notices);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> EditContent(Guid id, [FromBody] EditNoticeRequest request, CancellationToken ct)
    {
        await noticeService.EditContentAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken ct)
    {
        var residentId = User.GetResidentId();
        await noticeService.AcknowledgeAsync(id, residentId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "Admin,Trustee")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await noticeService.ArchiveAsync(id, ct);
        return NoContent();
    }
}
