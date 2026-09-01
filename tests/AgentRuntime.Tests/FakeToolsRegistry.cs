using AgentCore.Tools;

namespace AgentRuntime.Tests;

public sealed class FakeToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ITool tool)
    {
        _tools.Add(tool.Name, tool);
    }

    public ITool GetRequired(string name)
    {
        if (!_tools.TryGetValue(name, out var tool))
        {
            throw new KeyNotFoundException(
                $"Tool '{name}' is not registered.");
        }

        return tool;
    }

    public bool TryGet(
        string name,
        out ITool? tool)
    {
        return _tools.TryGetValue(name, out tool);
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
    {
        return _tools.Keys.ToArray();
    }
}