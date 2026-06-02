using FluentAssertions;
using Moq;
using UnitConnect.Application.Exceptions;
using UnitConnect.Application.Features.Notices;
using UnitConnect.Domain.Aggregates.Notices;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;

namespace UnitConnect.Tests.Application;

public sealed class NoticeServiceTests
{
    private readonly Mock<INoticeRepository> _repo = new();
    private readonly NoticeService _service;

    private static readonly Guid ComplexId  = Guid.NewGuid();
    private static readonly Guid PostedById = Guid.NewGuid();

    public NoticeServiceTests() => _service = new NoticeService(_repo.Object);

    private static Notice MakeNotice() =>
        Notice.Create(ComplexId, PostedById, "Water Outage", "Off 10am–2pm.");

    private void SetupSave() =>
        _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    // ── PublishAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_ValidRequest_ReturnsNewId()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Notice>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        SetupSave();

        var id = await _service.PublishAsync(ComplexId, PostedById,
            new PublishNoticeRequest("Water Outage", "Off 10am–2pm."));

        id.Should().NotBe(Guid.Empty);
    }

    // ── AcknowledgeAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task AcknowledgeAsync_ExistingNotice_Saves()
    {
        var notice = MakeNotice();
        _repo.Setup(r => r.GetByIdAsync(notice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notice);
        SetupSave();

        await _service.AcknowledgeAsync(notice.Id, Guid.NewGuid());

        _repo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task AcknowledgeAsync_NonExistentNotice_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Notice?)null);

        var act = () => _service.AcknowledgeAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── ArchiveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveAsync_PublishedNotice_Saves()
    {
        var notice = MakeNotice();
        _repo.Setup(r => r.GetByIdAsync(notice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notice);
        SetupSave();

        await _service.ArchiveAsync(notice.Id);

        _repo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    // ── GetPublishedByComplexAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetPublishedByComplexAsync_ReturnsResponses()
    {
        var notices = new List<Notice> { MakeNotice(), MakeNotice() };
        _repo.Setup(r => r.GetPublishedByComplexAsync(ComplexId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(notices);

        var result = await _service.GetPublishedByComplexAsync(ComplexId);

        result.Should().HaveCount(2);
    }
}
