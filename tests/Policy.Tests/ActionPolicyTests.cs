using AgentCore.Context;
using AgentCore.Models;
using Policy;

namespace Policy.Tests;

public sealed class ActionPolicyTests
{
    private readonly ActionPolicy _policy = new();

    private static AgentContext CreateContext()
    {
        return new AgentContext
        {
            ConversationId = "test-conversation",
            CustomerId = "test-customer"
        };
    }

    [Fact]
    public async Task EvaluateAsync_LookupOrder_AllowsAction()
    {
        var proposal = new ActionProposal
        {
            Name = "lookup_order",
            Parameters = new Dictionary<string, object?>
            {
                ["order_id"] = "12345"
            },
            Confidence = 0.95
        };

        var result = await _policy.EvaluateAsync(
            proposal,
            CreateContext());

        Assert.True(result.Allowed);
        Assert.False(result.RequiresHumanApproval);
        Assert.Empty(result.ValidationErrors);

        Assert.Contains(
            "POL-ORDER-LOOKUP-001",
            result.PolicyReferences);
    }

    [Fact]
    public async Task EvaluateAsync_UnknownAction_DeniesAction()
    {
        var proposal = new ActionProposal
        {
            Name = "unknown_action",
            Parameters = new Dictionary<string, object?>(),
            Confidence = 0.95
        };

        var result = await _policy.EvaluateAsync(
            proposal,
            CreateContext());

        Assert.False(result.Allowed);
        Assert.False(result.RequiresHumanApproval);

        Assert.Contains(
            "Action 'unknown_action' is not permitted.",
            result.ValidationErrors);

        Assert.Contains(
            "POL-ACTION-DENY-UNKNOWN",
            result.PolicyReferences);
    }

    [Fact]
    public async Task EvaluateAsync_DeleteAccount_DeniesAction()
    {
        var proposal = new ActionProposal
        {
            Name = "delete_account",
            Parameters = new Dictionary<string, object?>(),
            Confidence = 0.99
        };

        var result = await _policy.EvaluateAsync(
            proposal,
            CreateContext());

        Assert.False(result.Allowed);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task EvaluateAsync_RefundOrder_DeniesAction()
    {
        var proposal = new ActionProposal
        {
            Name = "refund_order",
            Parameters = new Dictionary<string, object?>(),
            Confidence = 0.99
        };

        var result = await _policy.EvaluateAsync(
            proposal,
            CreateContext());

        Assert.False(result.Allowed);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task EvaluateAsync_NullProposal_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _policy.EvaluateAsync(
                null!,
                CreateContext()));
    }

    [Fact]
    public async Task EvaluateAsync_NullContext_Throws()
    {
        var proposal = new ActionProposal
        {
            Name = "lookup_order",
            Parameters = new Dictionary<string, object?>(),
            Confidence = 0.95
        };

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _policy.EvaluateAsync(
                proposal,
                null!));
    }
}