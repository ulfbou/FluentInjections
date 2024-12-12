using Microsoft.EntityFrameworkCore;

namespace FluentInjections.Tests.Extensions;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds data into the provided DbContext using a custom seeding function.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="seedAction">The action to seed data into the context.</param>
    public static async Task Seed<TContext>(this TContext context, Action<TContext> seedAction)
        where TContext : DbContext
    {
        if (seedAction == null) throw new ArgumentNullException(nameof(seedAction));

        seedAction(context);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
