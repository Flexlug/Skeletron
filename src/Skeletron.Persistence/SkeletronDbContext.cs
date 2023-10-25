using Microsoft.EntityFrameworkCore;
using Skeletron.Domain;

namespace Skeletron.Persistence;

public class SkeletronDbContext: DbContext
{
    public DbSet<Joke> Jokes { get; set; }
    public DbSet<VkVideo> VkVideos { get; set; }
    
    public 
}