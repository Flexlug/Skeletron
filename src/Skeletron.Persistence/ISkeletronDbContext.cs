using Microsoft.EntityFrameworkCore;
using Skeletron.Domain;

namespace Skeletron.Persistence;

public interface ISkeletronDbContext
{
    public DbSet<Joke> Jokes { get; set; }
    public DbSet<VkVideo> VkVideos { get; set; }

    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}