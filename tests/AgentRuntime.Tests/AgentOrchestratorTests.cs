using AgentCore.Context;
using AgentCore.Models;
using AgentRuntime.Decisions;
using AgentRuntime.Models;
using AgentRuntime.Tools;
using AgentRuntime.Approval;
using AgentRuntime.Execution;


namespace AgentRuntime.Tests;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task RunAsync_UsesLLMProvider()
    {
        var llm = new FakeLLMProvider();
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();
        var policy = new FakePolicyEngine();
        var registry = new FakeToolRegistry();
        var toolExecutor = new ToolExecutor(registry);

        var approval = new FakeApprovalService();

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        Assert.Null(policy.LastProposal);
        Assert.Null(policy.LastContext);

        var request = new AgentRequest
        {
            Context = new AgentContext
            {
                ConversationId = "test-conversation"
            },

            Message = "Hello"
        };

        var result = await orchestrator.RunAsync(request);

        Assert.NotNull(llm.LastRequest);
        Assert.Equal("Hello", llm.LastRequest!.UserMessage);

        Assert.NotNull(result.Decision);
        Assert.Equal("general", result.Decision!.Intent);
        Assert.Equal(0.99, result.Decision.Confidence);
    }
    [Fact]
    public async Task RunAsync_DeniedAction_ReturnsActionDenied()
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();
        var policy = new FakePolicyEngine(allowed: false);
        var registry = new FakeToolRegistry();
        var toolExecutor = new ToolExecutor(registry);

        var approval = new FakeApprovalService();

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        var request = new AgentRequest
        {
            Context = new AgentCore.Context.AgentContext
            {
                ConversationId = "test-conversation"
            },
            Message = "Refund my order."
        };

        var result = await orchestrator.RunAsync(request);

        Assert.Equal(
            AgentRunStatus.ActionDenied,
            result.Status);

        Assert.NotNull(result.PolicyDecision);
        Assert.False(result.PolicyDecision!.Allowed);

        Assert.NotNull(policy.LastProposal);
        Assert.Equal(
            "refund_order",
            policy.LastProposal!.Name);
    }
    [Fact]
    public async Task RunAsync_DeniedAction_DoesNotExecuteTool() //denied execution
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var policy = new FakePolicyEngine(
            allowed: false);

        var approval = new FakeApprovalService();

        var fakeTool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(fakeTool);

        var toolExecutor = new ToolExecutor(registry);

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        var request = new AgentRequest
        {
            Context = new AgentCore.Context.AgentContext
            {
                ConversationId = "test-conversation"
            },
            Message = "Refund my order."
        };

        var result = await orchestrator.RunAsync(request);

        Assert.Equal(
            AgentRunStatus.ActionDenied,
            result.Status);

        Assert.False(
            result.PolicyDecision!.Allowed);

        Assert.False(fakeTool.WasExecuted);
    }
    [Fact]
    public async Task RunAsync_AllowedAction_ExecutesTool() //allowed execution
    {
        const string response = """
        {
        "intent": "test_action",
        "confidence": 0.99,
        "summary": "Customer requested an action.",
        "action": {
            "name": "fake_tool",
            "parameters": {
            "value": "hello"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var policy = new FakePolicyEngine(
            allowed: true);

        var approval = new FakeApprovalService();

        var fakeTool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(fakeTool);

        var toolExecutor = new ToolExecutor(registry);

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);


        var request = new AgentRequest
        {
            Context = new AgentCore.Context.AgentContext
            {
                ConversationId = "test-conversation"
            },
            Message = "Do the action."
        };

        var result = await orchestrator.RunAsync(request);

        Assert.Equal(
            AgentRunStatus.ActionExecuted,
            result.Status);

        Assert.True(fakeTool.WasExecuted);

        Assert.NotNull(fakeTool.LastRequest);

        Assert.Equal(
            "fake_tool",
            fakeTool.LastRequest!.ToolName);

        Assert.Equal(
            "hello",
            fakeTool.LastRequest.Parameters["value"]?.ToString());
    }
    [Fact]
    public async Task RunAsync_ApprovalRequired_ReturnsAwaitingHumanApproval() //approval required
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var policy = new FakePolicyEngine(
            allowed: true,
            requiresHumanApproval: true);

        var approval = new FakeApprovalService();

        var fakeTool = new FakeTool();

        var registry = new FakeToolRegistry();
                registry.Register(fakeTool);
        var toolExecutor = new ToolExecutor(registry);

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        var request = new AgentRequest
        {
            Context = new AgentContext
            {
                ConversationId = "conversation-001"
            },
            Message = "Please refund order ORD-123."
        };

        var result = await orchestrator.RunAsync(request);

        Assert.Equal(
            AgentRunStatus.AwaitingHumanApproval,
            result.Status);

        Assert.NotNull(result.PolicyDecision);

        Assert.True(
            result.PolicyDecision!.Allowed);

        Assert.True(
            result.PolicyDecision.RequiresHumanApproval);

        Assert.NotNull(result.ApprovalRequest);

        Assert.Equal(
            ApprovalStatus.Pending,
            result.ApprovalRequest!.Status);

        Assert.Equal(
            "refund_order",
            result.ApprovalRequest.ActionName);

        Assert.Equal(
            "conversation-001",
            result.ApprovalRequest.ConversationId);

        Assert.True(
            approval.CreateCalled);

        Assert.Null(result.ToolResult);

        Assert.False(
            fakeTool.WasExecuted);
    }
    [Fact]
    public async Task ResumeAsync_ApprovedAction_ExecutesTool()
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var policy = new FakePolicyEngine(
            allowed: true,
            requiresHumanApproval: true);

        var approval = new FakeApprovalService();

        var fakeTool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(
            new NamedFakeTool("refund_order", fakeTool));

        var toolExecutor = new ToolExecutor(registry);

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        // Initial request.
        var initialResult = await orchestrator.RunAsync(
            new AgentRequest
            {
                Context = context,
                Message = "Refund my order."
            });

        Assert.Equal(
            AgentRunStatus.AwaitingHumanApproval,
            initialResult.Status);

        Assert.NotNull(
            initialResult.ApprovalRequest);

        var approvalId =
            initialResult.ApprovalRequest!.ApprovalId;

        Assert.False(fakeTool.WasExecuted);

        // Human approval.
        var approved = await approval.ApproveAsync(
            approvalId);

        Assert.Equal(
            ApprovalStatus.Approved,
            approved.Status);

        // Resume.
        var result = await orchestrator.ResumeAsync(
            new ActionResumeRequest
            {
                ApprovalId = approvalId
            },
            context);

        Assert.Equal(
            AgentRunStatus.ActionExecuted,
            result.Status);

        Assert.NotNull(result.ToolResult);
        Assert.True(result.ToolResult!.Success);
        Assert.True(fakeTool.WasExecuted);
    }
    [Fact]
    public async Task ResumeAsync_PolicyDeniedAfterApproval_DoesNotExecuteTool()
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var policy = new FakePolicyEngine(
            allowed: true,
            requiresHumanApproval: true);

        var approval = new FakeApprovalService();

        var fakeTool = new FakeTool();

        var registry = new FakeToolRegistry();
        registry.Register(
            new NamedFakeTool("refund_order", fakeTool));

        var toolExecutor = new ToolExecutor(registry);

        var actionExecution = new ActionExecutionService(
            policy,
            toolExecutor,
            approval);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            actionExecution,
            approval);

        var context = new AgentContext
        {
            ConversationId = "conversation-001"
        };

        // Initial request creates a pending approval.
        var initialResult = await orchestrator.RunAsync(
            new AgentRequest
            {
                Context = context,
                Message = "Refund my order."
            });

        Assert.Equal(
            AgentRunStatus.AwaitingHumanApproval,
            initialResult.Status);

        var approvalId =
            initialResult.ApprovalRequest!.ApprovalId;

        // Human approves.
        var approved = await approval.ApproveAsync(
            approvalId);

        Assert.Equal(
            ApprovalStatus.Approved,
            approved.Status);

        // Policy changes before resume.
        policy.SetPolicy(
            allowed: false);

        // Resume must re-check policy.
        var result = await orchestrator.ResumeAsync(
            new ActionResumeRequest
            {
                ApprovalId = approvalId
            },
            context);

        Assert.Equal(
            AgentRunStatus.ActionDenied,
            result.Status);

        Assert.NotNull(
            result.PolicyDecision);

        Assert.False(
            result.PolicyDecision!.Allowed);

        Assert.Null(
            result.ToolResult);

        Assert.False(
            fakeTool.WasExecuted);
    }
}