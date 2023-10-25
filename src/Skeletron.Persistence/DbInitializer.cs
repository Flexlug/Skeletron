namespace Skeletron.Persistence;

public class DbInitializer
{
    public static void Initialize(SkeletronDbContext context)
    {
        context.Database.EnsureCreated();
    }
}