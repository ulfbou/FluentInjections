using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace FluentInjections;

public interface IFluentInjectionsApplication : IHost, IApplicationBuilder, IEndpointRouteBuilder, IAsyncDisposable { }
