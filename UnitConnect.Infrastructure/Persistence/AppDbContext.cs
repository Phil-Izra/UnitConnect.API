using Microsoft.EntityFrameworkCore;
using UnitConnect.Application.Abstractions;
using UnitConnect.Domain.Common;

namespace UnitConnect.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher dispatcher) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect and clear events before saving so a failed save doesn't leave stale events
        var events = ChangeTracker
            .Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        ChangeTracker
            .Entries<Entity>()
            .ToList()
            .ForEach(e => e.Entity.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var evt in events)
            await dispatcher.DispatchAsync(evt, cancellationToken);

        return result;
    }
}
