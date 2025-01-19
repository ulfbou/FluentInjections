using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using System.Text.Json;

namespace FluentInjections.Tests.Units.Configurator;

// EndpointConfiguratorTests validating the behavior of the EndpointConfigurator and related classes.
/*
1. ** Map Endpoint with Valid Parameters**
   - ** Arrange**: Create an instance of `EndpointConfigurator`.
   - ** Act**: Call `Map<TService, TRequest>` with valid parameters.
   - ** Assert**: Verify that the endpoint descriptor is added with the correct properties.

2. ** Map Endpoint with Null Pattern**
   - ** Arrange**: Create an instance of `EndpointConfigurator`.
   - ** Act**: Call `Map<TService, TRequest>` with a null pattern.
   - ** Assert**: Verify that an exception is thrown.

3. ** Map Endpoint with Null Handler**
   - ** Arrange**: Create an instance of `EndpointConfigurator`.
   - ** Act**: Call `Map<TService, TRequest>` with a null handler.
   - ** Assert**: Verify that an exception is thrown.

4. ** Require Authorization**
   - ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `RequireAuthorization`.
   - **Assert**: Verify that the `RequireAuthorization` property is set to true in the endpoint descriptor.

5. **Configure Validation**
   - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `ConfigureValidation` with a validation filter.
   - **Assert**: Verify that the validation filter type and instance are set in the endpoint descriptor.

6. **Set Endpoint Name**
   - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `WithName` with a valid name.
   - **Assert**: Verify that the `Name` property is set in the endpoint descriptor.

7. **Set Group Name**
   - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `WithGroup` with a valid group name.
   - **Assert**: Verify that the `Group` property is set in the endpoint descriptor.

8. **Set Error Handler**
   - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `WithErrorHandler` with a valid error handler.
   - **Assert**: Verify that the `ErrorHandler` property is set in the endpoint descriptor.

9. **Set Priority**
   - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
   - **Act**: Call `WithPriority` with a valid priority value.
   - **Assert**: Verify that the `Priority` property is set in the endpoint descriptor.

10. **Set Tag**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
    - **Act**: Call `WithTag` with a valid tag.
    - **Assert**: Verify that the `Tag` property is set in the endpoint descriptor.

11. **Set Timeout**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
    - **Act**: Call `WithTimeout` with a valid timeout value.
    - **Assert**: Verify that the `Timeout` property is set in the endpoint descriptor.

12. **Apply Group Policy**
    - **Arrange**: Create an instance of `EndpointConfigurator`, map an endpoint, and set a group name.
    - **Act**: Call `ApplyGroupPolicy` with the same group name and a configuration action.
    - **Assert**: Verify that the configuration action is applied to the endpoint descriptor.

13. **Configure All Endpoints**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map multiple endpoints.
    - **Act**: Call `ConfigureAll` with a configuration action.
    - **Assert**: Verify that the configuration action is applied to all endpoint descriptors.

14. **Build Endpoint Descriptor**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
    - **Act**: Call `Build` on the `EndpointsBuilder`.
    - **Assert**: Verify that the endpoint descriptor is added to the configurator.

15. **Add Metadata**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
    - **Act**: Call `TryAddMetadata` with a key-value pair.
    - **Assert**: Verify that the metadata is added to the endpoint descriptor.

16. **Retrieve Metadata**
    - **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint, then add metadata.
    - **Act**: Call `TryGetMetadata` with the key.
    - **Assert**: Verify that the correct metadata value is retrieved from the endpoint descriptor.

### Test Details

#### 1. Map Endpoint with Valid Parameters
- **Arrange**: Create an instance of `EndpointConfigurator`.
- **Act**: Call `Map<TService, TRequest>("api/test", EndpointMethod.Get, (service, request, context) => Task.FromResult(Results.Ok()))`.
- ** Assert**: Verify that the endpoint descriptor is added with the correct properties.

#### 2. Map Endpoint with Null Pattern
- ** Arrange**: Create an instance of `EndpointConfigurator`.
- ** Act**: Call `Map<TService, TRequest>(null, EndpointMethod.Get, (service, request, context) => Task.FromResult(Results.Ok()))`.
- ** Assert**: Verify that an exception is thrown.

#### 3. Map Endpoint with Null Handler
- ** Arrange**: Create an instance of `EndpointConfigurator`.
- ** Act**: Call `Map<TService, TRequest>("api/test", EndpointMethod.Get, null)`.
- ** Assert**: Verify that an exception is thrown.

#### 4. Require Authorization
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `RequireAuthorization`.
- **Assert**: Verify that the `RequireAuthorization` property is set to true in the endpoint descriptor.

#### 5. Configure Validation
- **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `ConfigureValidation<MyValidationFilter>()`.
- **Assert**: Verify that the validation filter type and instance are set in the endpoint descriptor.

#### 6. Set Endpoint Name
- **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithName("TestEndpoint")`.
- ** Assert**: Verify that the `Name` property is set in the endpoint descriptor.

#### 7. Set Group Name
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithGroup("TestGroup")`.
- ** Assert**: Verify that the `Group` property is set in the endpoint descriptor.

#### 8. Set Error Handler
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithErrorHandler(exception => Task.CompletedTask)`.
- ** Assert**: Verify that the `ErrorHandler` property is set in the endpoint descriptor.

#### 9. Set Priority
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithPriority(1)`.
- ** Assert**: Verify that the `Priority` property is set in the endpoint descriptor.

#### 10. Set Tag
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithTag("TestTag")`.
- ** Assert**: Verify that the `Tag` property is set in the endpoint descriptor.

#### 11. Set Timeout
- ** Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `WithTimeout(TimeSpan.FromMinutes(1))`.
- ** Assert**: Verify that the `Timeout` property is set in the endpoint descriptor.

#### 12. Apply Group Policy
- ** Arrange**: Create an instance of `EndpointConfigurator`, map an endpoint, and set a group name.
- ** Act**: Call `ApplyGroupPolicy("TestGroup", descriptor => descriptor.Priority = 5)`.
- ** Assert**: Verify that the configuration action is applied to the endpoint descriptor.

#### 13. Configure All Endpoints
- ** Arrange**: Create an instance of `EndpointConfigurator` and map multiple endpoints.
- **Act**: Call `ConfigureAll(descriptor => descriptor.Priority = 5)`.
- **Assert**: Verify that the configuration action is applied to all endpoint descriptors.

#### 14. Build Endpoint Descriptor
- **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `Build` on the `EndpointsBuilder`.
- **Assert**: Verify that the endpoint descriptor is added to the configurator.

#### 15. Add Metadata
- **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint.
- **Act**: Call `TryAddMetadata("Key", "Value")`.
- **Assert**: Verify that the metadata is added to the endpoint descriptor.

#### 16. Retrieve Metadata
- **Arrange**: Create an instance of `EndpointConfigurator` and map an endpoint, then add metadata.
- **Act**: Call `TryGetMetadata<string>("Key", out var value)`.
- **Assert**: Verify that the correct metadata value is retrieved from the endpoint descriptor.
*/
internal abstract partial class EndpointConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    : ConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    where TConfigurator : class, IEndpointConfigurator, ITestEndpointConfigurator
    where TServices : class, IServiceCollection, new()
    where TProvider : class, IServiceProvider
    where TFixture : class, IEndpointConfiguratorFixture<TConfigurator, TServices, TProvider>, IConfiguratorFixture<TConfigurator, TServices, TProvider>, new()
{
    public IServiceProvider ServiceProvider => _serviceProvider;
    protected readonly IServiceProvider _serviceProvider;

    public Mock<IEndpointRouteBuilder> EndpointRouteBuilderMock => _endpointRouteBuilderMock;
    protected readonly Mock<IEndpointRouteBuilder> _endpointRouteBuilderMock;

    public Mock<ILogger<EndpointConfigurator>> LoggerMock => _loggerMock;
    protected readonly Mock<ILogger<EndpointConfigurator>> _loggerMock;

    public Mock<IOptions<JsonSerializerOptions>> JsonSerializerOptionsMock => _jsonSerializerOptionsMock;
    protected readonly Mock<IOptions<JsonSerializerOptions>> _jsonSerializerOptionsMock;

    public Mock<WebApplication> ApplicationMock => _applicationMock;
    protected readonly Mock<WebApplication> _applicationMock;

    public EndpointConfigurator EndpointConfigurator => _endpointConfigurator;
    protected readonly EndpointConfigurator _endpointConfigurator;

    public EndpointConfiguratorTests()
    {
        _serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance))
                                                  .BuildServiceProvider();

        _endpointRouteBuilderMock = new Mock<IEndpointRouteBuilder>();
        _loggerMock = new Mock<ILogger<EndpointConfigurator>>();
        _jsonSerializerOptionsMock = new Mock<IOptions<JsonSerializerOptions>>();
        _applicationMock = new Mock<WebApplication>();
        _endpointConfigurator = new EndpointConfigurator(
            _applicationMock.Object,
            _loggerMock.Object);
    }

    public void Map_ShouldRegisterEndpointWithCorrectPatternAndMethod()
    {
        // Arrange
        var pattern = "/api/test";
        var method = EndpointMethod.GET;
        Func<ITestService, TestRequest, HttpContext, Task<IResult>> handler = (s, r, c) => Task.FromResult(Results.Ok());

        // Act
        _endpointConfigurator.Map<ITestService, TestRequest>(pattern, method, handler);
        _endpointRouteBuilderMock.Verify(x => x.MapMethods(It.Is<string>(p => p == pattern), It.Is<string[]>(m => m.Contains(method.ToString())), It.IsAny<RequestDelegate>()), Times.Once);
    }

    public void Map_ShouldRegisterEndpointWithCorrectHandler()
    {
        // Arrange
        var pattern = "/api/test";
        var method = EndpointMethod.GET;
        Func<ITestService, TestRequest, HttpContext, Task<IResult>> expectedHandler = (s, r, c) => Task.FromResult(Results.Ok());

        // Act
        _endpointConfigurator.Map<ITestService, TestRequest>(pattern, method, expectedHandler);

        // Assert
        _endpointRouteBuilderMock.Verify(x => x.MapMethods(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<RequestDelegate>()), Times.Once);
    }


    public interface ITestService { }
    public class TestService : ITestService { }
    public class TestRequest { }

    public class MockValidationFilter
    {
        public Task ValidateAsync(HttpContext context)
        {
            return Task.CompletedTask;
        }
    }
}
