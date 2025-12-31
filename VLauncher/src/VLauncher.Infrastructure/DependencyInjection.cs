using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VLauncher.Domain.Interfaces;
using VLauncher.Infrastructure.Data;
using VLauncher.Infrastructure.Repositories;
using VLauncher.Infrastructure.Services;

namespace VLauncher.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Active Directory
        services.Configure<ActiveDirectorySettings>(
            configuration.GetSection("ActiveDirectory"));
        services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

        return services;
    }
}
