using AgentCore.Models;
using AgentCore.Tools;

namespace Tools;

public sealed class LookupOrderTool : ITool
{
    private static readonly Dictionary<string, OrderRecord> Orders =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ORD-123"] = new OrderRecord(
                "ORD-123",
                "Shipped",
                59.90m),

            ["ORD-456"] = new OrderRecord(
                "ORD-456",
                "Processing",
                129.00m)
        };

    public string Name => "lookup_order";

    public Task<ToolResult> ExecuteAsync(
        ToolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Parameters.TryGetValue(
                "order_id",
                out var rawOrderId))
        {
            return Task.FromResult(
                new ToolResult
                {
                    Success = false,
                    Error = "Missing required parameter: order_id."
                });
        }

        var orderId = rawOrderId?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
        {
            return Task.FromResult(
                new ToolResult
                {
                    Success = false,
                    Error = "Parameter 'order_id' must not be empty."
                });
        }

        if (!Orders.TryGetValue(orderId, out var order))
        {
            return Task.FromResult(
                new ToolResult
                {
                    Success = false,
                    Error = $"Order '{orderId}' was not found."
                });
        }

        return Task.FromResult(
            new ToolResult
            {
                Success = true,
                Data = order
            });
    }

    private sealed record OrderRecord(
        string OrderId,
        string Status,
        decimal Total);
}