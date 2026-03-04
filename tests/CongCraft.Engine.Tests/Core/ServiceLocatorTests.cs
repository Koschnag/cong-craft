using CongCraft.Engine.Core;

namespace CongCraft.Engine.Tests.Core;

public class ServiceLocatorTests
{
    [Fact]
    public void Register_And_Get_ReturnsService()
    {
        var locator = new ServiceLocator();
        var service = new TestService();
        locator.Register(service);
        Assert.Same(service, locator.Get<TestService>());
    }

    [Fact]
    public void Get_Unregistered_Throws()
    {
        var locator = new ServiceLocator();
        Assert.Throws<InvalidOperationException>(() => locator.Get<TestService>());
    }

    [Fact]
    public void TryGet_Unregistered_ReturnsFalse()
    {
        var locator = new ServiceLocator();
        Assert.False(locator.TryGet<TestService>(out var service));
        Assert.Null(service);
    }

    [Fact]
    public void TryGet_Registered_ReturnsTrue()
    {
        var locator = new ServiceLocator();
        locator.Register(new TestService());
        Assert.True(locator.TryGet<TestService>(out var service));
        Assert.NotNull(service);
    }

    private class TestService { }
}
