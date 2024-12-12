namespace FluentInjections.Tests.Utilities
{
    internal class ArgumentGuard
    {
        internal static void NotNull(object? argument, string errorMessage)
        {
            if (argument is null) throw new ArgumentNullException(errorMessage);
        }
    }
}