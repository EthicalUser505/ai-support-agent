using AgentCore.Models;
using Tools;

namespace Tools.Tests;

public sealed class LookupOrderToolTests
{
    private readonly LookupOrderTool _tool = new();

    [Fact]
    public async Task ExecuteAsync_ExistingOrder_ReturnsOrder()
    {
        var request = new ToolRequest
        {
            ToolName = "lookup_order",
            Parameters = new Dictionary<string, object?>
            {
                ["order_id"] = "ORD-123"
            }
        };

        var result = await _tool.ExecuteAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_MissingOrderId_ReturnsFailure()
    {
        var request = new ToolRequest
        {
            ToolName = "lookup_order"
        };

        var result = await _tool.ExecuteAsync(request);

        Assert.False(result.Success);
        Assert.Equal(
            "Missing required parameter: order_id.",
            result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownOrder_ReturnsFailure()
    {
        var request = new ToolRequest
        {
            ToolName = "lookup_order",
            Parameters = new Dictionary<string, object?>
            {
                ["order_id"] = "ORD-999"
            }
        };

        var result = await _tool.ExecuteAsync(request);

        Assert.False(result.Success);
        Assert.Equal(
            "Order 'ORD-999' was not found.",
            result.Error);
    }
}