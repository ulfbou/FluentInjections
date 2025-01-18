namespace FluentInjections.Internal.Descriptors;

internal class EndpointDescriptor : IEndpointDescriptor
{
    public string Pattern { get; set; } = string.Empty;
    public EndpointMethod Method { get; set; }
    public Type ServiceType { get; set; } = typeof(object);
    public string? Name { get; set; }
    public string? Group { get; set; }
    public int Priority { get; set; }
    public string? Tag { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool RequireAuthorization { get; set; }
    public Type? ValidationFilterType { get; set; }
    public object? ValidationFilterInstance { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
