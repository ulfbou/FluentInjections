namespace FluentInjections.Extensions.Exceptions;

public static class ExceptionsExtensions
{
    /// <summary>
    /// Executes a function and falls back to a provided function if an exception occurs.
    /// </summary>
    public static T ExecuteWithFallback<T>(this Func<T> func, Func<Exception, T> fallbackFunc)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return fallbackFunc(ex);
        }
    }

    /// <summary>
    /// Retries a function based on a condition and delay, up to a specified number of attempts.
    /// </summary>
    public static T Retry<T>(this Func<T> func, int retryCount, Func<Exception, bool> retryCondition, TimeSpan delay)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (!retryCondition(ex) || i == retryCount - 1)
                {
                    throw;
                }

                Task.Delay(delay).Wait(); // Simulate asynchronous delay
            }
        }

        throw new Exception("Retry limit exceeded.");
    }

    /// <summary>
    /// Combines multiple exceptions into a single AggregateException with a custom message.
    /// </summary>
    public static AggregateException AggregateExceptions(this IEnumerable<Exception> exceptions, string message)
    {
        return new AggregateException(message, exceptions);
    }

    /// <summary>
    /// Returns a flattened list of all messages in the exception hierarchy.
    /// </summary>
    public static IEnumerable<string> GetExceptionMessages(this Exception exception)
    {
        var messages = new List<string>();

        while (exception is not null)
        {
            messages.Add(exception.Message);
            exception = exception.InnerException!;
        }

        return messages;
    }

    /// <summary>
    /// Executes an action and only handles exceptions of the specified types.
    /// </summary>
    public static void HandleExceptions(this Action action, params Type[] exceptionTypes)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (exceptionTypes.Any(t => ex.GetType().IsAssignableFrom(t)))
            {
                // Handle the exception here (e.g., log, notify)
            }
            else
            {
                throw;
            }
        }
    }
}