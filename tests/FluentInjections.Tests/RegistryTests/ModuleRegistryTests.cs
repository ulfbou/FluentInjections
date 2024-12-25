using FluentAssertions;

using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Middlewares;
using FluentInjections.Tests.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using System.Collections.Concurrent;

namespace FluentInjections.Tests.RegistryTests;

public class ModuleRegistryTests
{
    private readonly IServiceCollection _services;
    private readonly ModuleRegistry _registry;

    public ModuleRegistryTests()
    {
        _services = new ServiceCollection();
        _registry = new ModuleRegistry(_services);
    }

    [Fact]
    public void Apply_Should_Configure_Modules_That_Can_Handle_Configurator()
    {
        // Arrange
        var configuratorMock = new Mock<IConfigurator>();
        var moduleMock = new Mock<IConfigurableModule<IConfigurator>>();
        moduleMock.Setup(m => m.CanHandle<IConfigurator>()).Returns(true);
        moduleMock.Setup(m => m.Configure(configuratorMock.Object));

        _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Act
        _registry.Apply(configuratorMock.Object);

        // Assert
        moduleMock.Verify(m => m.Configure(configuratorMock.Object), Times.Once);
    }

    [Fact]
    public void Initialize_Should_Call_Initialize_On_Initializable_Modules()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>(MockBehavior.Strict);
        var initializableMock = moduleMock.As<IInitializable>();
        initializableMock.Setup(m => m.Initialize());

        _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Act
        _registry.Initialize();

        // Assert
        initializableMock.Verify(m => m.Initialize(), Times.Once);
    }

    [Fact]
    public void Register_Should_Add_Module_To_Registry()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();

        // Act
        _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Assert
        _registry.GetAllModules()
            .Should()
            .ContainSingle(m => m == moduleMock.Object);
    }

    [Fact]
    public void Register_Should_Throw_If_Module_Is_Already_Registered()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();
        _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Act
        Action act = () => _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Module of type {moduleMock.Object.GetType().Name} is already registered.");
    }

    [Fact]
    public void Unregister_Should_Remove_Module_From_Registry()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();
        _registry.Register<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Act
        _registry.Unregister<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Assert
        _registry.GetAllModules()
            .Should()
            .NotContain(moduleMock.Object);
    }

    [Fact]
    public void Unregister_Should_Throw_If_Module_Not_Registered()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();

        // Act
        Action act = () => _registry.Unregister<IModule<IConfigurator>, IConfigurator>(moduleMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Module of type {moduleMock.Object.GetType().Name} is not registered.");
    }

    [Fact]
    public void Register_With_Factory_Should_Create_And_Add_Module()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();

        // Act
        _registry.Register<IModule<IConfigurator>, IConfigurator>(() => moduleMock.Object);

        // Assert
        _registry.GetAllModules()
            .Should()
            .ContainSingle(m => m == moduleMock.Object);
    }

    [Fact]
    public void Register_With_Factory_And_Configure_Action_Should_Invoke_Configuration()
    {
        // Arrange
        var moduleMock = new Mock<IModule<IConfigurator>>();
        var configureActionMock = new Mock<Action<IModule<IConfigurator>>>();
        configureActionMock.Setup(a => a.Invoke(moduleMock.Object));

        // Act
        _registry.Register<IModule<IConfigurator>, IConfigurator>(() => moduleMock.Object, configureActionMock.Object);

        // Assert
        configureActionMock.Verify(a => a.Invoke(moduleMock.Object), Times.Once);
        _registry.GetAllModules()
            .Should()
            .ContainSingle(m => m == moduleMock.Object);
    }


    [Fact]
    public void RegisterModule_NullModule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _registry.Register<IModule<IConfigurator>, IConfigurator>(null!));
    }

    [Fact]
    public void RegisterModule_WithCondition_AddsModule()
    {
        // Arrange
        var configuratorMock = new Mock<IServiceConfigurator>();
        var moduleMock = new Mock<IServiceModule>();
        moduleMock.Setup(m => m.Configure(configuratorMock.Object)).Verifiable();

        // Act

        var serviceModulesField = _registry.GetType().GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(_registry) as ConcurrentDictionary<Type, List<IModule<IConfigurator>>>;

        Assert.NotNull(serviceModules);
        Assert.Single(serviceModules);
        Assert.IsType<TestServiceModule>(serviceModules.First().Value.First());
    }

    [Fact]
    public void RegisterModule_WithServiceModule_AddsModule()
    {
        var module = new TestServiceModule();
        var serviceModulesField = _registry.GetType().GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(_registry) as ConcurrentDictionary<Type, List<IModule<IConfigurator>>>;

        Assert.NotNull(serviceModules);
        var single = Assert.Single(serviceModules);
    }

    [Fact]
    public void RegisterModule_WithMiddlewareModule_AddsModule()
    {
        var module = new TestMiddlewareModule();

        var middlewareModulesField = _registry.GetType().GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var middlewareModules = middlewareModulesField?.GetValue(_registry) as ConcurrentDictionary<Type, List<IModule<IConfigurator>>>;

        Assert.NotNull(middlewareModules);
        Assert.Single(middlewareModules);
    }

    [Fact]
    public void RegisterModule_WithFactory_AddsModule()
    {
        var factoryModule = new TestServiceModule();

        var serviceModulesField = _registry.GetType().GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(_registry) as ConcurrentDictionary<Type, List<IModule<IConfigurator>>>;

        Assert.NotNull(serviceModules);
        Assert.Single(serviceModules);
        Assert.IsType<TestServiceModule>(serviceModules.First().Value.First());
    }

    [Fact]
    public void ApplyServiceModules_ConfiguresAllServiceModules()
    {
        var serviceConfiguratorMock = new Mock<IServiceConfigurator>();
        var module1 = new Mock<IServiceModule>();
        var module2 = new Mock<IServiceModule>();

        _registry.Apply(serviceConfiguratorMock.Object);

        module1.Verify(m => m.Configure(serviceConfiguratorMock.Object), Times.Once);
        module2.Verify(m => m.Configure(serviceConfiguratorMock.Object), Times.Once);
    }

    [Fact]
    public void ApplyMiddlewareModules_ConfiguresAllMiddlewareModules()
    {
        var middlewareConfiguratorMock = new Mock<IMiddlewareConfigurator>();
        var module1 = new Mock<IMiddlewareModule>();
        var module2 = new Mock<IMiddlewareModule>();

        _registry.Apply(middlewareConfiguratorMock.Object);

        module1.Verify(m => m.Configure(middlewareConfiguratorMock.Object), Times.Once);
        module2.Verify(m => m.Configure(middlewareConfiguratorMock.Object), Times.Once);
    }

    [Fact]
    public void InitializeModules_InitializesAllModules()
    {
        var initializableModule1 = new Mock<IServiceModule>();
        var initializableModule2 = new Mock<IMiddlewareModule>();

        initializableModule1.As<IInitializable>().Setup(m => m.Initialize()).Verifiable();
        initializableModule2.As<IInitializable>().Setup(m => m.Initialize()).Verifiable();

        _registry.Initialize();

        initializableModule1.As<IInitializable>().Verify(m => m.Initialize(), Times.Once);
        initializableModule2.As<IInitializable>().Verify(m => m.Initialize(), Times.Once);
    }

    [Fact]
    public void UnregisterModule_WithRegisteredModule_RemovesModule()
    {
        var module = new TestModule();
        _registry.Unregister<IServiceConfigurator>(typeof(TestModule), module);
        var serviceModulesField = _registry.GetType().GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(_registry) as ConcurrentDictionary<Type, List<IModule<IConfigurator>>>;
        Assert.NotNull(serviceModules);
        Assert.Empty(serviceModules);
    }

    [Fact]
    public void ApplyServiceModules_HandlesEmptyRegistryGracefully()
    {
        // No service modules registered
        var serviceConfiguratorMock = new Mock<IServiceConfigurator>();

        var exception = Record.Exception(() => _registry.Apply(serviceConfiguratorMock.Object));

        Assert.Null(exception);
    }

    [Fact]
    public void ApplyMiddlewareModules_HandlesEmptyRegistryGracefully()
    {
        // No middleware modules registered
        var middlewareConfiguratorMock = new Mock<IMiddlewareConfigurator>();

        var exception = Record.Exception(() => _registry.Apply(middlewareConfiguratorMock.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void InitializeModules_NoInitializableModules_DoesNothing()
    {
        // No modules registered
        var exception = Record.Exception(() => _registry.Initialize());

        Assert.Null(exception);
    }

    [Fact]
    public void UnregisterModule_WithUnregisteredModule_ThrowsException()
    {
        var module = new TestModule();
        Assert.Throws<InvalidOperationException>(() => _registry.Unregister<IModule<IConfigurator>, IConfigurator>(module));
    }

    [Fact]
    public void InitializeModules_HandlesInitializationExceptions()
    {
        var faultyModule = new FaultyModule();

        var exception = Record.Exception(() => _registry.Initialize());

        Assert.NotNull(exception);
        Assert.IsType<AggregateException>(exception);
    }

    [Fact]
    public void RegisterModule_DuplicateModule_ThrowsInvalidOperationException()
    {
        var module = new TestModule();

        _registry.Register<IServiceConfigurator>(typeof(TestModule), module);

        Assert.Throws<InvalidOperationException>(() => _registry.Register<IServiceConfigurator>(typeof(TestModule), module));
    }

    internal sealed class TestModule : Module<IServiceConfigurator>, IServiceModule
    {
        public override void Configure(IServiceConfigurator configurator) => configurator.Bind<ITestService>().To<TestService>();
    }

    internal sealed class FaultyModule : Module<IServiceConfigurator>, IServiceModule, IInitializable
    {
        public override void Configure(IServiceConfigurator configurator) => throw new InvalidOperationException("Configuration failed");

        public void Initialize() => throw new InvalidOperationException("Initialization failed");
    }

    internal sealed class TestServiceModule : Module<IServiceConfigurator>, IServiceModule
    {
        public override void Configure(IServiceConfigurator configurator) => configurator.Bind<ITestService>().To<TestService>();
    }

    internal sealed class TestMiddlewareModule : Module<IMiddlewareConfigurator>, IMiddlewareModule
    {
        public override void Configure(IMiddlewareConfigurator configurator) => configurator.UseMiddleware<TestMiddleware>();
    }
}
