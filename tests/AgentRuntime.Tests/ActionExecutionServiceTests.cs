using AgentCore.Context;
using AgentCore.Models;
using AgentRuntime.Approval;
using AgentRuntime.Decisions;
using AgentRuntime.Execution;
using AgentRuntime.Models;
using AgentRuntime.Tools;

namespace AgentRuntime.Tests;

public sealed class ActionExecutionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_AllowedAction_ExecutesTool()
    {
        var policy = new FakePolicyEngine(
            allowed: true);

        var approval = new FakeApprovalService();

        var tool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(tool);

        var toolExecutor = new ToolExecutor(registry);

        var service = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var proposal = new ActionProposal
        {
            Name = "fake_tool",
            Parameters = new Dictionary<string, object?>
            {
                ["value"] = "hello"
            },
            Confidence = 0.95
        };

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        var result = await service.ExecuteAsync(
            "conversation-001",
            proposal,
            context);

        Assert.True(result.PolicyDecision.Allowed);
        Assert.False(
            result.PolicyDecision.RequiresHumanApproval);

        Assert.NotNull(result.ToolResult);
        Assert.True(result.ToolResult!.Success);

        Assert.False(
            approval.CreateCalled);

        Assert.True(
            tool.WasExecuted);
    }

    [Fact]
    public async Task ExecuteAsync_DeniedAction_DoesNotExecuteTool()
    {
        var policy = new FakePolicyEngine(
            allowed: false);

        var approval = new FakeApprovalService();

        var tool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(tool);

        var toolExecutor = new ToolExecutor(registry);

        var service = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var proposal = new ActionProposal
        {
            Name = "fake_tool",
            Parameters = new Dictionary<string, object?>(),
            Confidence = 0.99
        };

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        var result = await service.ExecuteAsync(
            "conversation-001",
            proposal,
            context);

        Assert.False(
            result.PolicyDecision.Allowed);

        Assert.False(
            result.PolicyDecision.RequiresHumanApproval);

        Assert.Null(result.ToolResult);
        Assert.False(approval.CreateCalled);
        Assert.False(tool.WasExecuted);
    }

    [Fact]
    public async Task ExecuteAsync_ApprovalRequired_CreatesApprovalAndDoesNotExecuteTool()
    {
        var policy = new FakePolicyEngine(
            allowed: true,
            requiresHumanApproval: true);

        var approval = new FakeApprovalService();

        var tool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(tool);

        var toolExecutor = new ToolExecutor(registry);

        var service = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var proposal = new ActionProposal
        {
            Name = "refund_order",
            Parameters = new Dictionary<string, object?>
            {
                ["order_id"] = "ORD-123"
            },
            Confidence = 0.99
        };

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        var result = await service.ExecuteAsync(
            "conversation-001",
            proposal,
            context);

        Assert.True(
            result.PolicyDecision.Allowed);

        Assert.True(
            result.PolicyDecision.RequiresHumanApproval);

        Assert.NotNull(
            result.ApprovalRequest);

        Assert.Equal(
            "approval-test-001",
            result.ApprovalRequest!.ApprovalId);

        Assert.Equal(
            ApprovalStatus.Pending,
            result.ApprovalRequest.Status);

        Assert.True(
            approval.CreateCalled);

        Assert.Null(result.ToolResult);

        Assert.False(
            tool.WasExecuted);
    }

    [Fact]
    public async Task ExecuteAsync_NullProposal_Throws()
    {
        var policy = new FakePolicyEngine();
        var approval = new FakeApprovalService();

        var registry = new FakeToolRegistry();
        var toolExecutor = new ToolExecutor(registry);

        var service = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.ExecuteAsync(
                "conversation-001",
                null!,
                context));
    }

    [Fact]
    public async Task ExecuteAsync_NullContext_Throws()
    {
        var policy = new FakePolicyEngine();
        var approval = new FakeApprovalService();

        var registry = new FakeToolRegistry();
        var toolExecutor = new ToolExecutor(registry);

        var service = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var proposal = new ActionProposal
        {
            Name = "fake_tool",
            Confidence = 0.95
        };

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.ExecuteAsync(
                "conversation-001",
                proposal,
                null!));
    }
}