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
        if (LastRequest is null ||
            LastRequest.ApprovalId != approvalId)
        {
            return Task.FromResult<ApprovalRequest?>(null);
        }

        return Task.FromResult<ApprovalRequest?>(
            LastRequest);
    }

    public Task<ApprovalRequest> ApproveAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        var request = GetExistingRequest(approvalId);

        LastRequest = new ApprovalRequest
        {
            ApprovalId = request.ApprovalId,
            ConversationId = request.ConversationId,
            ActionName = request.ActionName,
            Parameters = request.Parameters,
            Reason = request.Reason,
            Status = ApprovalStatus.Approved
        };

        return Task.FromResult(LastRequest);
    }

    public Task<ApprovalRequest> RejectAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        var request = GetExistingRequest(approvalId);

        LastRequest = new ApprovalRequest
        {
            ApprovalId = request.ApprovalId,
            ConversationId = request.ConversationId,
            ActionName = request.ActionName,
            Parameters = request.Parameters,
            Reason = request.Reason,
            Status = ApprovalStatus.Rejected
        };

        return Task.FromResult(LastRequest);
    }

    private ApprovalRequest GetExistingRequest(
        string approvalId)
    {
        if (LastRequest is null ||
            LastRequest.ApprovalId != approvalId)
        {
            throw new KeyNotFoundException(
                $"Approval '{approvalId}' was not found.");
        }

        if (LastRequest.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Approval '{approvalId}' is already {LastRequest.Status}.");
        }

        return LastRequest;
    }
}