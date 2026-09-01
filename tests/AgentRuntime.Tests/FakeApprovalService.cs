using AgentCore.Approval;
using AgentCore.Models;

namespace AgentRuntime.Tests;

public sealed class FakeApprovalService : IApprovalService
{
    public bool CreateCalled { get; private set; }

    public ApprovalRequest? LastRequest { get; private set; }

    public Task<ApprovalRequest> CreateAsync(
        string conversationId,
        ActionProposal proposal,
        string reason,
        CancellationToken cancellationToken = default)
    {
        CreateCalled = true;

        LastRequest = new ApprovalRequest
        {
            ApprovalId = "approval-test-001",
            ConversationId = conversationId,
            ActionName = proposal.Name,
            Parameters = proposal.Parameters,
            Reason = reason,
            Status = ApprovalStatus.Pending
        };

        return Task.FromResult(LastRequest);
    }

    public Task<ApprovalRequest?> GetAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LastRequest);
    }

    public Task<ApprovalRequest> ApproveAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalRequest> RejectAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}