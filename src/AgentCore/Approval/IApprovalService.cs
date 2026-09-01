using AgentCore.Models;

namespace AgentCore.Approval;

public interface IApprovalService
{
    Task<ApprovalRequest> CreateAsync(
        string conversationId,
        ActionProposal proposal,
        string reason,
        CancellationToken cancellationToken = default);

    Task<ApprovalRequest?> GetAsync(
        string approvalId,
        CancellationToken cancellationToken = default);

    Task<ApprovalRequest> ApproveAsync(
        string approvalId,
        CancellationToken cancellationToken = default);

    Task<ApprovalRequest> RejectAsync(
        string approvalId,
        CancellationToken cancellationToken = default);
}