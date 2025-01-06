
namespace FluentInjections.Core;

// Base class for Disposable objects to handle the dispose pattern, and ensuring that the object has not been disposed before being used
/// <summary>
/// Base class for Disposable objects to handle the dispose pattern, and ensuring that the object has not been disposed before being used.
/// </summary>
public abstract class Disposable : IDisposable, IAsyncDisposable
{
    private bool _disposed;
    private object _lock = new object();

    /// <summary>
    /// Ensures that the object has not been disposed before being used.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
    protected void EnsureNotDisposed()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            Dispose(true);
            _disposed = true;
        }
    }

    /// <summary>
    /// Disposes of the object.
    /// </summary>
    protected virtual void Dispose(bool disposing) { }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return default;
            }
            Dispose(true);
            _disposed = true;
        }
        return default;
    }
}
