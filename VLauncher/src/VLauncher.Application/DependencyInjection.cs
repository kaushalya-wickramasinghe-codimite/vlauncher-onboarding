using Microsoft.Extensions.DependencyInjection;

namespace VLauncher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}


// This scans VLauncher.Application.dll and registers:
        // - GetAllUsersQueryHandler
        // - GetUserByIdQueryHandler
        // - CreatePendingUserCommandHandler
        // - ... all handlers automatically!