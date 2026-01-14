using Microsoft.EntityFrameworkCore;

namespace MemberOpsAPI.Data;

public static class DatabaseManager
{
    public static async Task ResetDatabase(AppDbContext context)
    {
        Console.WriteLine("🗑️  Dropping existing database...");
        await context.Database.EnsureDeletedAsync();

        Console.WriteLine("🔨 Creating new database with migrations...");
        await context.Database.MigrateAsync();

        Console.WriteLine("🌱 Seeding data...");
        DbSeeder.SeedData(context);

        Console.WriteLine("✅ Database reset complete!");
    }
}
