using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.Persistence;

public interface IMigrationRunner
{
    Task ApplyAsync(CancellationToken cancellationToken);
}

public sealed class EfMigrationRunner(IoBuildDbContext dbContext) : IMigrationRunner
{
    public async Task ApplyAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (dbContext.Database.IsRelational())
        {
            var statements = new[]
            {
                "ALTER TABLE profiles ADD COLUMN PhoneNumber VARCHAR(50) NULL;",
                "ALTER TABLE profiles ADD COLUMN Address VARCHAR(255) NULL;",
                "ALTER TABLE profiles ADD COLUMN SecondEmail VARCHAR(150) NULL;",
                "ALTER TABLE profiles ADD COLUMN Age INT NULL;",
                "ALTER TABLE profiles ADD COLUMN PhotoUrl VARCHAR(2000) NULL;"
            };

            foreach (var sql in statements)
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                }
                catch
                {
                    // Ignored: column likely already exists.
                }
            }
        }
    }
}
