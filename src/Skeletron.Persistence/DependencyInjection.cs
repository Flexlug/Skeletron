using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Skeletron.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DbConnection"];
        services.AddDbContext<SkeletronDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<SkeletronDbContext>(provider => provider.GetService<SkeletronDbContext>());
        return services;
    }
}