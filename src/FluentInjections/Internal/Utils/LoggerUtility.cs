using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Utils;

/// <summary>
/// Utility class for logging.
/// </summary>
/// <remarks>
/// This class is used to log messages to the console.
/// </remarks>
public static class LoggerUtility
{
    private static ILoggerFactory _loggerFactory = new LoggerFactory();

    /// <summary>
    /// Create a logger.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <returns>The logger instance.</returns>
    public static ILogger<T> CreateLogger<T>() => new Logger<T>(_loggerFactory);

    /// <summary>
    /// Create a logger.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);

    /// <summary>
    /// Create a logger.
    /// </summary>
    /// <param name="type">The type of the logger.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger CreateLogger(Type type) => _loggerFactory.CreateLogger(type);
}
