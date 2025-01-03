// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Extensions;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Units.ExtensionTests;

public class IsCompatibleToTests
{
    [Theory]
    [InlineData(typeof(BaseClass), typeof(BaseClass), true)]
    [InlineData(typeof(DerivedClass), typeof(BaseClass), true)]
    [InlineData(typeof(Interface1), typeof(Interface1), true)]
    [InlineData(typeof(ClassImplementingInterface1), typeof(Interface1), true)]
    [InlineData(typeof(GenericClass<int>), typeof(GenericClass<int>), true)]
    [InlineData(typeof(GenericClass<string>), typeof(GenericClass<string>), true)]
    [InlineData(typeof(Nullable<int>), typeof(int?), true)] // Nullable<T> to Nullable<T>
    [InlineData(typeof(int), typeof(int?), true)] // T to Nullable<T>
    [InlineData(typeof(List<int>), typeof(IEnumerable<int>), true)] // Covariance
    [InlineData(typeof(List<string>), typeof(IEnumerable<object>), true)] // Contravariance
    [InlineData(typeof(Action), typeof(Delegate), true)] // Base class of delegates
    [InlineData(typeof(Func<int>), typeof(Delegate), true)] // Base class of delegates
    [InlineData(typeof(IServiceBinding), typeof(IConfigurator<IServiceBinding>), true)]
    [InlineData(typeof(IServiceModule), typeof(IConfigurableModule<IServiceConfigurator>), true)]
    [InlineData(typeof(IMiddlewareModule), typeof(IConfigurableModule<IMiddlewareConfigurator>), true)]
    [InlineData(typeof(IServiceBinding<>), typeof(IServiceBinding<>), true)]
    [InlineData(typeof(IMiddlewareBinding<>), typeof(IMiddlewareBinding<>), true)]
    [InlineData(typeof(IServiceBinding<>), typeof(IBinding), true)]
    [InlineData(typeof(IMiddlewareBinding<>), typeof(IBinding), true)]
    [InlineData(typeof(MiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>), typeof(AutofacMiddlewareConfigurator), true)]
    [InlineData(typeof(ServiceConfigurator), typeof(AutofacServiceConfigurator), true)]
    [InlineData(typeof(Configurator<IServiceBinding, ServiceBindingDescriptor>), typeof(ServiceConfigurator), true)]
    [InlineData(typeof(Configurator<IServiceBinding, ServiceBindingDescriptor>), typeof(NetCoreServiceConfigurator), true)]
    [InlineData(typeof(IConfigurator<IServiceBinding>), typeof(ServiceConfigurator), true)]
    [InlineData(typeof(IServiceProvider), typeof(NetCoreServiceProvider), true)]
    [InlineData(typeof(IKeyedServiceProvider), typeof(NetCoreServiceProvider), true)]
    [InlineData(typeof(MiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>), typeof(NetCoreMiddlewareConfigurator<IApplicationBuilder>), true)]
    [InlineData(typeof(IConfigurator), typeof(NetCoreServiceConfigurator), true)]
    [InlineData(typeof(IDisposable), typeof(AutofacServiceConfigurator), true)]
    [InlineData(typeof(IDisposable), typeof(ServiceConfigurator), true)]
    [InlineData(typeof(IMiddlewareConfigurator), typeof(AutofacMiddlewareConfigurator), true)]
    [InlineData(typeof(IMiddlewareConfigurator), typeof(NetCoreMiddlewareConfigurator<ContainerBuilder>), true)]
    [InlineData(typeof(IServiceConfigurator), typeof(NetCoreServiceConfigurator), true)]
    public void IsCompatibleWith_Should_Pass(Type sourceType, Type targetType, bool expectedResult)
    {
        // Act
        var result = sourceType.IsCompatibleWith(targetType);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(typeof(BaseClass), typeof(DerivedClass), false)]
    [InlineData(typeof(Interface1), typeof(ClassImplementingInterface1), false)]
    [InlineData(typeof(GenericClass<int>), typeof(GenericClass<string>), false)]
    [InlineData(typeof(int?), typeof(int), false)] // Nullable<T> to T (not always compatible)
    [InlineData(typeof(IServiceBinding), typeof(IServiceBinding<>), false)]
    [InlineData(typeof(IMiddlewareBinding), typeof(IMiddlewareBinding<>), false)]
    [InlineData(typeof(IConfigurableModule<>), typeof(IModule<>), false)]
    [InlineData(typeof(ServiceConfigurator), typeof(Configurator<IServiceBinding, ServiceBindingDescriptor>), false)]
    [InlineData(typeof(AutofacServiceConfigurator), typeof(MiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>), false)]
    [InlineData(typeof(NetCoreServiceConfigurator), typeof(MiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>), false)]
    [InlineData(typeof(AutofacMiddlewareConfigurator), typeof(ServiceConfigurator), false)]
    public void IsCompatibleWith_Should_Fail(Type sourceType, Type targetType, bool expectedResult)
    {
        // Act
        var result = sourceType.IsCompatibleWith(targetType);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
public class BaseClass { }
public class DerivedClass : BaseClass { }
public interface Interface1 { }
public class ClassImplementingInterface1 : Interface1 { }
public class GenericClass<T> { }
public interface IRepository<T> { }
public interface IProcessor<T> where T : BaseClass { }
public class ConcreteRepository<T> : IRepository<T> { }
public class ConstrainedClass<T> where T : DerivedClass { }
