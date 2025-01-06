// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace FluentInjections.Extensions.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Logs a message at the specified level only if a condition is met.
    /// </summary>
    public static void LogIf(this ILogger logger, Func<bool> condition, LogLevel level, string message, params object[] args)
    {
        if (condition())
        {
            logger.Log(level, message, args);
        }
    }

    /// <summary>
    /// Logs a message with a correlation ID for tracking requests across systems.
    /// </summary>
    public static void LogWith(this ILogger logger, string correlationId, LogLevel level, string message, params object[] args)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.Log(level, message, args);
        }
    }

    /// <summary>
    /// Logs an exception with a custom message and log level.
    /// </summary>
    public static void LogException(this ILogger logger, Exception exception, LogLevel level, string message, params object[] args)
    {
        logger.Log(level, exception, message, args);
    }

    /// <summary>
    /// Attaches a context dictionary to log entries for additional metadata.
    /// </summary>
    public static IDisposable? WithEnrichmentContext<TState>(this ILogger<TState> logger, IDictionary<string, object> context)
    {
        return logger.BeginScope(context);
    }
}