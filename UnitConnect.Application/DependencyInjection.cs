using Microsoft.Extensions.DependencyInjection;
using UnitConnect.Application.EventHandlers;
using UnitConnect.Application.Features.Complexes;
using UnitConnect.Application.Features.Listings;
using UnitConnect.Application.Features.Notices;
using UnitConnect.Application.Features.Residents;
using UnitConnect.Domain.Events;

namespace UnitConnect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IComplexService, ComplexService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<INoticeService, NoticeService>();

        services.AddScoped<IDomainEventHandler<NoticePublishedEvent>, NoticePublishedEventHandler>();
        services.AddScoped<IDomainEventHandler<ResidentRegisteredEvent>, ResidentRegisteredEventHandler>();

        return services;
    }
}
