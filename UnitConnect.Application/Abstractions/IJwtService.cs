using UnitConnect.Domain.Aggregates.Residents;

namespace UnitConnect.Application.Abstractions;

public interface IJwtService
{
    string GenerateToken(Resident resident);
}
