using AgentCore.Models;
using AgentCore.Tools;
using Tools;

namespace Tools.Tests;

public sealed class ToolRegistryTests
{
    [Fact]
    public void Register_AndGetRegisteredTool_Works()
    {
        var registry = new ToolRegistry();
        var tool = new LookupOrderTool();

        registry.Register(tool);

        var resolved = registry.GetRequired("lookup_order");

        Assert.Same(tool, resolved);
    }

    [Fact]
    public void TryGet_UnknownTool_ReturnsFalse()
    {
        var registry = new ToolRegistry();

        var found = registry.TryGet(
            "does_not_exist",
            out var tool);

        Assert.False(found);
        Assert.Null(tool);
    }

    [Fact]
    public void Register_DuplicateTool_Throws()
    {
        var registry = new ToolRegistry();

        registry.Register(new LookupOrderTool());

        Assert.Throws<InvalidOperationException>(
            () => registry.Register(new LookupOrderTool()));
    }
}