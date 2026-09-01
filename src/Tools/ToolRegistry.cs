using AgentCore.Tools;

namespace Tools;

public sealed class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ITool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);

        if (string.IsNullOrWhiteSpace(tool.Name))
        {
            throw new ArgumentException(
                "Tool name must not be empty.",
                nameof(tool));
        }

        if (!_tools.TryAdd(tool.Name, tool))
        {
            throw new InvalidOperationException(
                $"A tool named '{tool.Name}' is already registered.");
        }
    }

    public ITool GetRequired(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tool name must not be empty.",
                nameof(name));
        }

        if (!_tools.TryGetValue(name, out var tool))
        {
            throw new KeyNotFoundException(
                $"Tool '{name}' is not registered.");
        }

        return tool;
    }

    public bool TryGet(string name, out ITool? tool)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            tool = null;
            return false;
        }

        return _tools.TryGetValue(name, out tool);
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
    {
        return _tools.Keys.ToArray();
    }
}