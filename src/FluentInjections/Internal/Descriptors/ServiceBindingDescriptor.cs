
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Descriptors
{
    public class ServiceBindingDescriptor
    {
        public Type BindingType { get; }

        public Type? ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public object? Instance { get; set; }
        public string? Name { get; internal set; }
        public object? Parameters { get; internal set; }
        public Func<IServiceProvider, object>? Factory { get; internal set; }
        public Action<object>? ConfigureOptions { get; internal set; }
        public Action<object>? Configure { get; internal set; }

        public ServiceBindingDescriptor(Type bindingType)
        {
            BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
            Lifetime = ServiceLifetime.Transient;
        }

    }

    public class ServiceBindingDescriptor<TService>() : ServiceBindingDescriptor(typeof(TService)) where TService : class
    {
    }
}
