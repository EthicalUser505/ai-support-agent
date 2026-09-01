using System.Collections.Concurrent;
using AgentCore.Approval;
using AgentCore.Models;

namespace AgentRuntime.Approval;

public sealed class InMemoryApprovalService : IApprovalService
{
    private readonly ConcurrentDictionary<
        string,
        ApprovalRequest> _requests = new();

    public Task<ApprovalRequest> CreateAsync(
        string conversationId,
        ActionProposal proposal,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            conversationId);

        ArgumentNullException.ThrowIfNull(proposal);

        var request = new ApprovalRequest
        {
            ApprovalId = Guid.NewGuid().ToString("N"),
            ConversationId = conversationId,
            ActionName = proposal.Name,
            Parameters = proposal.Parameters,
            Reason = reason,
            Status = ApprovalStatus.Pending
        };

        if (!_requests.TryAdd(
                request.ApprovalId,
                request))
        {
            throw new InvalidOperationException(
                "Unable to create approval request.");
        }

        return Task.FromResult(request);
    }

    public Task<ApprovalRequest?> GetAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        _requests.TryGetValue(
            approvalId,
            out var request);

        return Task.FromResult(request);
    }

    public Task<ApprovalRequest> ApproveAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        return SetStatusAsync(
            approvalId,
            ApprovalStatus.Approved);
    }

    public Task<ApprovalRequest> RejectAsync(
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        return SetStatusAsync(
            approvalId,
            ApprovalStatus.Rejected);
    }

    private Task<ApprovalRequest> SetStatusAsync(
        string approvalId,
        ApprovalStatus status)
    {
        if (!_requests.TryGetValue(
                approvalId,
                out var existing))
        {
            throw new KeyNotFoundException(
                $"Approval '{approvalId}' was not found.");
        }

        if (existing.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Approval '{approvalId}' is already {existing.Status}.");
        }

        var updated = new ApprovalRequest
        {
            ApprovalId = existing.ApprovalId,
            ConversationId = existing.ConversationId,
            ActionName = existing.ActionName,
            Parameters = existing.Parameters,
            Reason = existing.Reason,
            Status = status
        };

        _requests[approvalId] = updated;

        return Task.FromResult(updated);
    }
}