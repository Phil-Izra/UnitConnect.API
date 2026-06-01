using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitConnect.Application.Abstractions;
using UnitConnect.Domain.Repositories;
using UnitConnect.Infrastructure.DomainEvents;
using UnitConnect.Infrastructure.Persistence;
using UnitConnect.Infrastructure.Persistence.Repositories;
using UnitConnect.Infrastructure.Services;

namespace UnitConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IComplexRepository, ComplexRepository>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IResidentInviteRepository, ResidentInviteRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        return services;
    }
}
