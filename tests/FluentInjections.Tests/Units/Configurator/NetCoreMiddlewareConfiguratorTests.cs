namespace FluentInjections.Tests.Units.Configurator;

public class NetCoreMiddlewareConfiguratorTests
{
    private readonly InternalNetCoreMiddlewareConfiguratorTests _internal = new InternalNetCoreMiddlewareConfiguratorTests();

    [Fact]
    public void Constructor_ShouldInitializeWithLogger()
    {
        _internal.Constructor_ShouldInitializeWithLogger();
    }

    [Fact]
    public void SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode()
    {
        _internal.SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode();
    }

    [Fact]
    public void UseMiddleware_ShouldAddMiddlewareDescriptor()
    {
        _internal.UseMiddleware_ShouldAddMiddlewareDescriptor();
    }

    [Fact]
    public void RemoveMiddleware_ShouldRemoveMiddlewareDescriptor()
    {
        _internal.RemoveMiddleware_ShouldRemoveMiddlewareDescriptor();
    }

    [Fact]
    public void ApplyGroupPolicy_ShouldApplyConfigurationToGroup()
    {
        _internal.ApplyGroupPolicy_ShouldApplyConfigurationToGroup();
    }

    [Fact]
    public void ConfigureAll_ShouldApplyConfigurationToAllMiddlewares()
    {
        _internal.ConfigureAll_ShouldApplyConfigurationToAllMiddlewares();
    }

    [Fact]
    public void Register_ShouldInvokeRegisterMethodForEachDescriptor()
    {
        _internal.Register_ShouldInvokeRegisterMethodForEachDescriptor();
    }

    [Fact]
    public void ValidateBindings_ShouldIdentifyAndHandleDuplicates()
    {
        _internal.ValidateBindings_ShouldIdentifyAndHandleDuplicates();
    }

    [Fact]
    public void OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly()
    {
        _internal.OrderMiddlewareDescriptors_ShouldOrderMiddlewaresCorrectly();
    }

    [Fact]
    public void MergeDescriptors_ShouldUpdatePropertiesCorrectly()
    {
        _internal.MergeDescriptors_ShouldUpdatePropertiesCorrectly();
    }
}
