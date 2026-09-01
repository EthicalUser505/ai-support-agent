namespace AgentCore.Tools;

public interface IToolRegistry
{
    ITool GetRequired(string name);

    bool TryGet(string name, out ITool? tool);

    IReadOnlyCollection<string> GetRegisteredNames();
}