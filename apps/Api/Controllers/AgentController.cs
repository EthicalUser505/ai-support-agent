using AgentCore.Approval;
using AgentCore.Context;
using AgentCore.Models;
using AgentRuntime;
using AgentRuntime.Models;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/agent")]
public sealed class AgentController : ControllerBase
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IApprovalService _approvalService;

    public AgentController(
        IAgentOrchestrator orchestrator,
        IApprovalService approvalService)
    {
        _orchestrator = orchestrator;
        _approvalService = approvalService;
    }

    [HttpPost("message")]
    public async Task<ActionResult<AgentMessageResponse>> SendMessage(
        [FromBody] AgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = new AgentContext
        {
            ConversationId = request.ConversationId,
            CustomerId = request.CustomerId
        };

        var result = await _orchestrator.RunAsync(
            new AgentRequest
            {
                Context = context,
                Message = request.Message
            },
            cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("approvals/{approvalId}/approve")]
    public async Task<ActionResult<AgentMessageResponse>> Approve(
        string approvalId,
        CancellationToken cancellationToken)
    {
        var approval = await _approvalService.ApproveAsync(
            approvalId,
            cancellationToken);

        var context = new AgentContext
        {
            ConversationId = approval.ConversationId
        };

        var result = await _orchestrator.ResumeAsync(
            new ActionResumeRequest
            {
                ApprovalId = approval.ApprovalId
            },
            context,
            cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("approvals/{approvalId}/reject")]
    public async Task<ActionResult<ApprovalRequest>> Reject(
        string approvalId,
        CancellationToken cancellationToken)
    {
        var approval = await _approvalService.RejectAsync(
            approvalId,
            cancellationToken);

        return Ok(approval);
    }

    private static AgentMessageResponse ToResponse(
        AgentRunResult result)
    {
        return new AgentMessageResponse
        {
            Status = result.Status,
            Response = result.Response,
            Decision = result.Decision,
            PolicyDecision = result.PolicyDecision,
            ToolResult = result.ToolResult,
            ApprovalRequest = result.ApprovalRequest
        };
    }
}