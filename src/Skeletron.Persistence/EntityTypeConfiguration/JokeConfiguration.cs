using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeletron.Domain;

namespace Skeletron.Persistence.EntityTypeConfiguration;

public class JokeConfigurationn : IEntityTypeConfiguration<Joke>
{
    public void Configure(EntityTypeBuilder<Joke> builder)
    {
        builder.HasKey(joke => joke.Id);
        builder.HasIndex(joke => joke.Id).IsUnique();
    }
}