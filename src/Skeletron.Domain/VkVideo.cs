namespace Skeletron.Domain;

public class VkVideo
{
    public Guid Id { get; set; }
    public long OwnerId { get; set; }
    public long VideoId { get; set; }
    public string DiscordUrl { get; set; }
}