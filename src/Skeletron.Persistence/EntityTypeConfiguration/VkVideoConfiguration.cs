using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeletron.Domain;

namespace Skeletron.Persistence.EntityTypeConfiguration;

public class VkVideoConfiguration : IEntityTypeConfiguration<VkVideo>
{
    public void Configure(EntityTypeBuilder<VkVideo> builder)
    {
        builder.HasKey(video => video.Id);
        builder.HasIndex(video => video.Id).IsUnique();
    }
}