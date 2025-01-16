using FluentInjections.Internal;

using Microsoft.Extensions.Hosting;

namespace FluentInjections;

public interface IFluentInjectionsBuilder : IHostApplicationBuilder
{
    IFluentInjectionsApplication Build();
}
