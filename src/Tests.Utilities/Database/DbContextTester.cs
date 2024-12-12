using Microsoft.EntityFrameworkCore;

namespace FluentInjections.Tests.Utilities;

public static class DbContextTester
{
    /// <summary>
    /// Executes a test action within the scope of a DbContext and rolls back any changes.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="createContext">A factory function to create a DbContext instance.</param>
    /// <param name="testAction">The action to test against the DbContext.</param>
    public static void TestWithRollback<TContext>(
        Func<TContext> createContext,
        Action<TContext> testAction)
        where TContext : DbContext
    {
        if (createContext == null) throw new ArgumentNullException(nameof(createContext));
        if (testAction == null) throw new ArgumentNullException(nameof(testAction));

        using var context = createContext();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            testAction(context);
            // Do not commit to ensure rollback after test.
        }
        finally
        {
            transaction.Rollback();
        }
    }
}