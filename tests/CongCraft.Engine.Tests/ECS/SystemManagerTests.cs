using CongCraft.Engine.Core;
using CongCraft.Engine.ECS.Systems;

namespace CongCraft.Engine.Tests.ECS;

public class SystemManagerTests
{
    [Fact]
    public void Systems_ExecuteInPriorityOrder()
    {
        var manager = new SystemManager();
        var log = new List<string>();

        manager.Register(new LogSystem("C", 30, log));
        manager.Register(new LogSystem("A", 10, log));
        manager.Register(new LogSystem("B", 20, log));

        var services = new ServiceLocator();
        manager.InitializeAll(services);
        manager.UpdateAll(new GameTime(0.016, 0.016, 1));

        Assert.Equal(new[] { "A", "B", "C" }, log);
    }

    [Fact]
    public void UpdateAll_BeforeInitialize_DoesNothing()
    {
        var manager = new SystemManager();
        var log = new List<string>();
        manager.Register(new LogSystem("A", 10, log));

        // Not initialized yet
        manager.UpdateAll(new GameTime(0.016, 0.016, 1));
        Assert.Empty(log);
    }

    [Fact]
    public void Dispose_DisposesAllSystems()
    {
        var manager = new SystemManager();
        var log = new List<string>();
        var sys = new LogSystem("A", 10, log);
        manager.Register(sys);

        var services = new ServiceLocator();
        manager.InitializeAll(services);
        manager.Dispose();

        Assert.True(sys.IsDisposed);
    }

    private class LogSystem : ISystem
    {
        private readonly string _name;
        private readonly List<string> _log;
        public int Priority { get; }
        public bool IsDisposed { get; private set; }

        public LogSystem(string name, int priority, List<string> log)
        {
            _name = name;
            Priority = priority;
            _log = log;
        }

        public void Initialize(ServiceLocator services) { }
        public void Update(GameTime time) => _log.Add(_name);
        public void Render(GameTime time) { }
        public void Dispose() => IsDisposed = true;
    }
}
