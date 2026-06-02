using FluentAssertions;
using Moq;
using UnitConnect.Application.Abstractions;
using UnitConnect.Application.Exceptions;
using UnitConnect.Application.Features.Residents;
using UnitConnect.Domain.Aggregates.Residents;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Repositories;
using UnitConnect.Domain.ValueObjects;

namespace UnitConnect.Tests.Application;

public sealed class ResidentServiceTests
{
    private readonly Mock<IResidentRepository>       _residentRepo = new();
    private readonly Mock<IResidentInviteRepository> _inviteRepo   = new();
    private readonly Mock<IEmailService>             _emailService = new();
    private readonly Mock<IPasswordHasher>           _hasher       = new();
    private readonly Mock<IJwtService>               _jwt          = new();
    private readonly ResidentService _service;

    private static readonly Guid ComplexId = Guid.NewGuid();

    public ResidentServiceTests() =>
        _service = new ResidentService(
            _residentRepo.Object, _inviteRepo.Object,
            _emailService.Object, _hasher.Object, _jwt.Object);

    private static Resident MakeResident() =>
        Resident.Register(ComplexId, "Jane", "Doe",
            Email.Create("jane@example.com"), "hash", "A1");

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidInvite_ReturnsNewResidentId()
    {
        var invite = ResidentInvite.Create(
            ComplexId, Email.Create("jane@example.com"), "A1", ResidentRole.Owner, Guid.NewGuid());

        _inviteRepo.Setup(r => r.GetByTokenAsync(invite.Token, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(invite);
        _residentRepo.Setup(r => r.EmailExistsInComplexAsync("jane@example.com", ComplexId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("password123")).Returns("bcrypt_hash");
        _residentRepo.Setup(r => r.AddAsync(It.IsAny<Resident>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
        _residentRepo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

        var id = await _service.RegisterAsync(
            new RegisterResidentRequest(invite.Token, "Jane", "Doe", "password123"));

        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task RegisterAsync_InvalidToken_ThrowsNotFoundException()
    {
        _inviteRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((ResidentInvite?)null);

        var act = () => _service.RegisterAsync(new RegisterResidentRequest("bad-token", "J", "D", "pw"));
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenAndResident()
    {
        var resident = MakeResident();
        _residentRepo.Setup(r => r.GetByEmailAsync("jane@example.com", ComplexId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(resident);
        _hasher.Setup(h => h.Verify("password123", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(resident)).Returns("jwt_token");

        var result = await _service.LoginAsync(new LoginRequest(ComplexId, "jane@example.com", "password123"));

        result.Token.Should().Be("jwt_token");
        result.Resident.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsNotFoundException()
    {
        var resident = MakeResident();
        _residentRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(resident);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = () => _service.LoginAsync(new LoginRequest(ComplexId, "jane@example.com", "wrong"));
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*password*");
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsNotFoundException()
    {
        _residentRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Resident?)null);

        var act = () => _service.LoginAsync(new LoginRequest(ComplexId, "unknown@email.com", "pw"));
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── UpdateProfileAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_ExistingResident_Saves()
    {
        var resident = MakeResident();
        _residentRepo.Setup(r => r.GetByIdAsync(resident.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(resident);
        _residentRepo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

        await _service.UpdateProfileAsync(resident.Id, new UpdateProfileRequest("John", "Smith"));

        _residentRepo.Verify(r => r.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistentResident_ThrowsNotFoundException()
    {
        _residentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Resident?)null);

        var act = () => _service.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest("J", "S"));
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
