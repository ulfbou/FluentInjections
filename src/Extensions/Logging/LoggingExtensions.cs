using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Extensions.Logging;

public static class LoggerExtensions
{
    /// <summary>
    /// Adds a custom enricher to the logging context.
    /// </summary>
    /// <typeparam name="TState">The type of the log event state.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="enricher">The enricher function.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger<TState> WithEnricher<TState>(this ILogger<TState> logger, Func<LogEntry, LogEntry> enricher)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (enricher == null)
        {
            throw new ArgumentNullException(nameof(enricher));
        }

        return new EnrichedLogger<TState>(logger, enricher);
    }

    /// <summary>
    /// Adds a custom enricher to the logging context asynchronously.
    /// </summary>
    /// <typeparam name="TState">The type of the log event state.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="enricher">The asynchronous enricher function.</param>
    /// <returns>The logger instance.</returns>
    public static async Task<ILogger<TState>> WithEnricherAsync<TState>(this ILogger<TState> logger, Func<LogEntry, Task<LogEntry>> enricher)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (enricher == null)
        {
            throw new ArgumentNullException(nameof(enricher));
        }

        return new EnrichedLogger<TState>(logger, enricher);
    }

    /// <summary>
    /// Adds additional enrichment to the log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the log event state.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="additionalEnrichment">The action to perform additional enrichment.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger<TState> WithAdditionalEnrichment<TState>(this ILogger<TState> logger, Action<LogEntry> additionalEnrichment)
    {
        return logger.WithEnricher(logEntry =>
        {
            additionalEnrichment(logEntry);
            return logEntry;
        });
    }

    /// <summary>
    /// Combines multiple enrichers into a single enricher.
    /// </summary>
    /// <typeparam name="TState">The type of the log event state.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="enrichers">An array of enricher functions.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger<TState> WithCompositeEnricher<TState>(this ILogger<TState> logger, params Func<LogEntry, LogEntry>[] enrichers)
    {
        return logger.WithEnricher(logEntry =>
        {
            foreach (var enricher in enrichers)
            {
                logEntry = enricher(logEntry);
            }
            return logEntry;
        });
    }

    /// <summary>
    /// Creates an enricher function based on configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the log event state.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="enricherFactory">A function that creates an enricher based on the configuration.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The logger instance.</returns>
    public static ILogger<TState> WithConfigurableEnricher<TState>(this ILogger<TState> logger, Func<IConfiguration, Func<LogEntry, LogEntry>> enricherFactory, IConfiguration configuration)
    {
        if (enricherFactory == null)
        {
            throw new ArgumentNullException(nameof(enricherFactory));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var enricher = enricherFactory(configuration);
        return logger.WithEnricher(enricher);
    }

    private class LogEntry
    {
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public object State { get; }
        public Exception Exception { get; }
        public Func<object, Exception, string> Formatter { get; }
        public DateTime Timestamp { get; }
        public string CorrelationId { get; }

        public LogEntry(LogLevel logLevel, EventId eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
            Formatter = formatter;
            Timestamp = DateTime.UtcNow;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }

    private class EnrichedLogger<TState> : ILogger<TState>
    {
        private readonly ILogger<TState> _innerLogger;
        private readonly Func<LogEntry, LogEntry> _synchronousEnricher;
        private readonly Func<LogEntry, Task<LogEntry>> _asynchronousEnricher;

        public EnrichedLogger(ILogger<TState> innerLogger, Func<LogEntry, LogEntry> synchronousEnricher)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _synchronousEnricher = synchronousEnricher ?? throw new ArgumentNullException(nameof(synchronousEnricher));
        }

        public EnrichedLogger(ILogger<TState> innerLogger, Func<LogEntry, Task<LogEntry>> asynchronousEnricher)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _asynchronousEnricher = asynchronousEnricher ?? throw new ArgumentNullException(nameof(asynchronousEnricher));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _innerLogger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _innerLogger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logEntry = new LogEntry(logLevel, eventId, state, exception, formatter);

            if (_synchronousEnricher != null)
            {
                logEntry = _synchronousEnricher(logEntry);
            }
            else if (_asynchronousEnricher != null)
            {
                var enrichedLogEntry = _asynchronousEnricher(logEntry).ConfigureAwait(false).GetAwaiter().GetResult();
                logEntry = enrichedLogEntry;
            }

            _innerLogger.Log(logEntry.LogLevel, logEntry.EventId, logEntry.State, logEntry.Exception, logEntry.Formatter);
        }
    }
}
