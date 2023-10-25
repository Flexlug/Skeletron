using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Skeletron.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string dbString)
    {
        services.AddDbContext<SkeletronDbContext>(options =>
        {
            options.UseNpgsql(dbString);
        });
        services.AddScoped<ISkeletronDbContext>(provider => provider.GetService<SkeletronDbContext>());
        return services;
    }
}