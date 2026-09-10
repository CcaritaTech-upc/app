using System.Data;
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
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync(cancellationToken);

            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'profiles';";
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    existingColumns.Add(reader.GetString(0));
                }
            }
            finally
            {
                if (shouldClose) await connection.CloseAsync();
            }

            var columnsToAdd = new (string Name, string Type)[]
            {
                ("PhoneNumber", "VARCHAR(50) NULL"),
                ("Address", "VARCHAR(255) NULL"),
                ("SecondEmail", "VARCHAR(150) NULL"),
                ("PhotoUrl", "LONGTEXT NULL")
            };

            foreach (var (colName, colType) in columnsToAdd)
            {
                if (!existingColumns.Contains(colName))
                {
                    await dbContext.Database.ExecuteSqlRawAsync($"ALTER TABLE profiles ADD COLUMN {colName} {colType};", cancellationToken);
                }
            }

            if (existingColumns.Contains("PhotoUrl"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE profiles MODIFY COLUMN PhotoUrl LONGTEXT NULL;", cancellationToken);
            }
            if (existingColumns.Contains("CloudinaryReference"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE profiles MODIFY COLUMN CloudinaryReference LONGTEXT NULL;", cancellationToken);
            }
        }
    }
}
