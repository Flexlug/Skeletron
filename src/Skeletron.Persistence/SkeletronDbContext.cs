using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skeletron.Domain;
using Skeletron.Persistence.EntityTypeConfiguration;

namespace Skeletron.Persistence;

public class SkeletronDbContext: DbContext, ISkeletronDbContext
{
    public DbSet<Joke> Jokes { get; set; }
    public DbSet<VkVideo> VkVideos { get; set; }

    public SkeletronDbContext(DbContextOptions<SkeletronDbContext> options)
        : base(options) {  }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new JokeConfigurationn());
        builder.ApplyConfiguration(new VkVideoConfiguration());
        base.OnModelCreating(builder);
    }
}