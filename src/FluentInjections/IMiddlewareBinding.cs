using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Represents a middleware binding that provides methods to bind and configure middleware components within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define middleware bindings.
/// </remarks>
public interface IMiddlewareBinding : IBinding
{
}
