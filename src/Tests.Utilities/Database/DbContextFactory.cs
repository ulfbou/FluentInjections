using Microsoft.EntityFrameworkCore;

namespace FluentInjections.Tests.Utilities;

public static class DbContextFactory
{
    /// <summary>
    /// Creates a new DbContext instance with an in-memory database.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="databaseName">Optional unique name for the database instance.</param>
    /// <param name="configureOptions">Optional action to configure additional options.</param>
    /// <returns>A new DbContext instance.</returns>
    public static TContext CreateInMemory<TContext>(
        string? databaseName = null,
        Action<DbContextOptionsBuilder>? configureOptions = null)
        where TContext : DbContext, new()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());

        configureOptions?.Invoke(optionsBuilder);

        return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
    }
}
