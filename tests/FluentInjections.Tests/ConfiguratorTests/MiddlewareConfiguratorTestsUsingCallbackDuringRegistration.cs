using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Modules;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Modules;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FluentInjections.Tests.ConfiguratorTests;

// The purpose of using callbacks during registration is to easily verify that the registration has been completed and executed correctly.
public class MiddlewareConfiguratorTestsUsingCallbackDuringRegistration
{
    public MiddlewareConfiguratorTestsUsingCallbackDuringRegistration()
    {
    }

    [Fact]
    public void Middleware_Should_Be_Registered()
    {
    }

    [Fact]
    public void Middleware_Should_Be_Registered_With_Correct_Priority()
    {
    }

    [Fact]
    public void Middleware_Should_Be_Registered_With_Correct_Group()
    {
    }

    [Fact]
    public void Middleware_Should_Be_Registered_With_Correct_Condition()
    {
    }

    [Fact]
    public void Middleware_Should_Be_Registered_With_Correct_Timeout()
    {
    }
}
