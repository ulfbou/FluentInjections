namespace FluentInjections;

public interface IEndpointDescriptor
{
    Func<Exception, Task>? ErrorHandler { get; set; }
    string? Group { get; set; }
    Dictionary<string, object> Metadata { get; set; }
    EndpointMethod Method { get; set; }
    string? Name { get; set; }
    string Pattern { get; set; }
    int Priority { get; set; }
    bool RequireAuthorization { get; set; }
    Type ServiceType { get; set; }
    string? Tag { get; set; }
    TimeSpan? Timeout { get; set; }
    object? ValidationFilterInstance { get; set; }
    Type? ValidationFilterType { get; set; }
}
